using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using FreeGameIsAFreeGame.Core;
using FreeGameIsAFreeGame.Core.Models;
using NLog;

namespace FreeGameIsAFreeGame.Scraper.EpicGames
{
    public class EpicGamesScraper : IScraper
    {
        private const string GRAPH_QUERY =
            "{\"query\":\"query searchStoreQuery($allowCountries: String, $category: String, $count: Int, $country: String!, $keywords: String, $locale: String, $namespace: String, $sortBy: String, $sortDir: String, $start: Int, $tag: String, $withPrice: Boolean = false, $withPromotions: Boolean = false) {\\n  Catalog {\\n    searchStore(allowCountries: $allowCountries, category: $category, count: $count, country: $country, keywords: $keywords, locale: $locale, namespace: $namespace, sortBy: $sortBy, sortDir: $sortDir, start: $start, tag: $tag) {\\n      elements {\\n        title\\n        id\\n        namespace\\n        description\\n        effectiveDate\\n        keyImages {\\n          type\\n          url\\n        }\\n        seller {\\n          id\\n          name\\n        }\\n        productSlug\\n        urlSlug\\n        url\\n        items {\\n          id\\n          namespace\\n        }\\n        customAttributes {\\n          key\\n          value\\n        }\\n        categories {\\n          path\\n        }\\n        price(country: $country) @include(if: $withPrice) {\\n          totalPrice {\\n            discountPrice\\n            originalPrice\\n            voucherDiscount\\n            discount\\n            currencyCode\\n            currencyInfo {\\n              decimals\\n            }\\n            fmtPrice(locale: $locale) {\\n              originalPrice\\n              discountPrice\\n              intermediatePrice\\n            }\\n          }\\n          lineOffers {\\n            appliedRules {\\n              id\\n              endDate\\n              discountSetting {\\n                discountType\\n              }\\n            }\\n          }\\n        }\\n        promotions(category: $category) @include(if: $withPromotions) {\\n          promotionalOffers {\\n            promotionalOffers {\\n              startDate\\n              endDate\\n              discountSetting {\\n                discountType\\n                discountPercentage\\n              }\\n            }\\n          }\\n          upcomingPromotionalOffers {\\n            promotionalOffers {\\n              startDate\\n              endDate\\n              discountSetting {\\n                discountType\\n                discountPercentage\\n              }\\n            }\\n          }\\n        }\\n      }\\n      paging {\\n        count\\n        total\\n      }\\n    }\\n  }\\n}\\n\",\"variables\":{\"category\":\"freegames\",\"sortBy\":\"effectiveDate\",\"sortDir\":\"asc\",\"count\":1000,\"country\":\"NL\",\"allowCountries\":\"NL\",\"locale\":\"en-US\",\"withPrice\":true,\"withPromotions\":true}}";

        private const string GRAPH_URL = "https://www.epicgames.com/store/backend/graphql-proxy";

        string IScraper.Identifier => "EpicGames";

        private readonly IBrowsingContext context;
        private readonly ILogger logger;

        public EpicGamesScraper()
        {
            context = BrowsingContext.New(Configuration.Default
                .WithDefaultLoader()
                .WithDefaultCookies());

            logger = LogManager.GetLogger(GetType().FullName);
        }

        async Task<IEnumerable<IDeal>> IScraper.Scrape(CancellationToken token)
        {
            string content = await GetGraphContent(token);
            if (token.IsCancellationRequested)
                return null;

            GraphData graphData = GraphData.FromJson(content);
            IEnumerable<IDeal> deals = ParseGraphData(graphData);
            if (token.IsCancellationRequested)
                return null;

            return deals;
        }

        private IEnumerable<IDeal> ParseGraphData(GraphData graphData)
        {
            List<IDeal> deals = new List<IDeal>();

            foreach (Element element in graphData.Data.Catalog.SearchStore.Elements)
            {
                PromotionalOfferPromotionalOffer promotionalOffer =
                    element.Promotions?.PromotionalOffers.FirstOrDefault()?.PromotionalOffers?.FirstOrDefault();
                if (promotionalOffer == null)
                {
                    logger.Info($"{element.Title} has no promotion available");
                    continue;
                }

                if (promotionalOffer.DiscountSetting.DiscountPercentage != 0)
                {
                    logger.Info($"{element.Title} is not free");
                    continue;
                }

                logger.Info($"Adding {element.Title}");
                deals.Add(new Deal()
                {
                    Discount = 100,
                    Title = element.Title,
                    Link = $"https://www.epicgames.com/store/en-US/product/{element.ProductSlug}",
                    Image = GetImage(element),
                    Start = GetStartDate(element),
                    End = GetEndDate(element),
                });
            }

            return deals;
        }

        private async Task<string> GetGraphContent(CancellationToken token)
        {
            await using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(GRAPH_QUERY));

            Url url = Url.Create(GRAPH_URL);
            DocumentRequest request = DocumentRequest.Post(url, stream, MimeTypeNames.ApplicationJson);
            request.Headers.Add("X-Request-With", "XMLHttpRequest");

            IDocument document = await context.OpenAsync(request, token);
            if (token.IsCancellationRequested)
                return null;

            string content = document.Body.Text();
            return content;
        }

        private string GetImage(Element element)
        {
            KeyImage image = element.KeyImages.FirstOrDefault(x => x.Type == "DieselStoreFrontTall");
            if (image != null)
                return image.Url;
            if (element.KeyImages.Count == 1)
                return string.Empty;
            return element.KeyImages.First(x => x.Type != "DieselStoreFrontTall").Url;
        }

        private DateTime GetStartDate(Element element)
        {
            PromotionalOfferPromotionalOffer promotion = GetPromotion(element);
            return promotion.StartDate.UtcDateTime;
        }

        private DateTime GetEndDate(Element element)
        {
            PromotionalOfferPromotionalOffer promotion = GetPromotion(element);
            return promotion.EndDate.UtcDateTime;
        }

        private PromotionalOfferPromotionalOffer GetPromotion(Element element)
        {
            return element.Promotions.PromotionalOffers?.FirstOrDefault()?.PromotionalOffers?.FirstOrDefault() ??
                   element.Promotions.UpcomingPromotionalOffers.FirstOrDefault()?.PromotionalOffers?.FirstOrDefault();
        }
    }
}
