using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FreeGameIsAFreeGame.Scraper.EpicGames
{
    using J = JsonPropertyAttribute;

    public partial class GraphData
    {
        [J("data")] public Data Data { get; set; }
        [J("extensions")] public Extensions Extensions { get; set; }
    }

    public partial class Data
    {
        [J("Catalog")] public Catalog Catalog { get; set; }
    }

    public partial class Catalog
    {
        [J("searchStore")] public SearchStore SearchStore { get; set; }
    }

    public partial class SearchStore
    {
        [J("elements")] public List<Element> Elements { get; set; }
        [J("paging")] public Paging Paging { get; set; }
    }

    public partial class Element
    {
        [J("title")] public string Title { get; set; }
        [J("id")] public string Id { get; set; }
        [J("namespace")] public string Namespace { get; set; }
        [J("description")] public string Description { get; set; }
        [J("effectiveDate")] public DateTimeOffset EffectiveDate { get; set; }
        [J("keyImages")] public List<KeyImage> KeyImages { get; set; }
        [J("seller")] public Seller Seller { get; set; }
        [J("productSlug")] public string ProductSlug { get; set; }
        [J("urlSlug")] public string UrlSlug { get; set; }
        [J("url")] public object Url { get; set; }
        [J("items")] public List<Item> Items { get; set; }
        [J("customAttributes")] public List<CustomAttribute> CustomAttributes { get; set; }
        [J("categories")] public List<Category> Categories { get; set; }
        [J("price")] public Price Price { get; set; }
        [J("promotions")] public Promotions Promotions { get; set; }
    }

    public partial class Category
    {
        [J("path")] public string Path { get; set; }
    }

    public partial class CustomAttribute
    {
        [J("key")] public string Key { get; set; }
        [J("value")] public string Value { get; set; }
    }

    public partial class Item
    {
        [J("id")] public string Id { get; set; }
        [J("namespace")] public string Namespace { get; set; }
    }

    public partial class KeyImage
    {
        [J("type")] public string Type { get; set; }
        [J("url")] public string Url { get; set; }
    }

    public partial class Price
    {
        [J("totalPrice")] public TotalPrice TotalPrice { get; set; }
        [J("lineOffers")] public List<LineOffer> LineOffers { get; set; }
    }

    public partial class LineOffer
    {
        [J("appliedRules")] public List<AppliedRule> AppliedRules { get; set; }
    }

    public partial class AppliedRule
    {
        [J("id")] public string Id { get; set; }
        [J("endDate")] public DateTimeOffset EndDate { get; set; }
        [J("discountSetting")] public AppliedRuleDiscountSetting DiscountSetting { get; set; }
    }

    public partial class AppliedRuleDiscountSetting
    {
        [J("discountType")] public string DiscountType { get; set; }
    }

    public partial class TotalPrice
    {
        [J("discountPrice")] public long DiscountPrice { get; set; }
        [J("originalPrice")] public long OriginalPrice { get; set; }
        [J("voucherDiscount")] public long VoucherDiscount { get; set; }
        [J("discount")] public long Discount { get; set; }
        [J("currencyCode")] public string CurrencyCode { get; set; }
        [J("currencyInfo")] public CurrencyInfo CurrencyInfo { get; set; }
        [J("fmtPrice")] public FmtPrice FmtPrice { get; set; }
    }

    public partial class CurrencyInfo
    {
        [J("decimals")] public long Decimals { get; set; }
    }

    public partial class FmtPrice
    {
        [J("originalPrice")] public string OriginalPrice { get; set; }
        [J("discountPrice")] public string DiscountPrice { get; set; }
        [J("intermediatePrice")] public string IntermediatePrice { get; set; }
    }

    public partial class Promotions
    {
        [J("promotionalOffers")] public List<PromotionalOffer> PromotionalOffers { get; set; }
        [J("upcomingPromotionalOffers")] public List<PromotionalOffer> UpcomingPromotionalOffers { get; set; }
    }

    public partial class PromotionalOffer
    {
        [J("promotionalOffers")] public List<PromotionalOfferPromotionalOffer> PromotionalOffers { get; set; }
    }

    public partial class PromotionalOfferPromotionalOffer
    {
        [J("startDate")] public DateTimeOffset StartDate { get; set; }
        [J("endDate")] public DateTimeOffset EndDate { get; set; }
        [J("discountSetting")] public PromotionalOfferDiscountSetting DiscountSetting { get; set; }
    }

    public partial class PromotionalOfferDiscountSetting
    {
        [J("discountType")] public string DiscountType { get; set; }
        [J("discountPercentage")] public long DiscountPercentage { get; set; }
    }

    public partial class Seller
    {
        [J("id")] public string Id { get; set; }
        [J("name")] public string Name { get; set; }
    }

    public partial class Paging
    {
        [J("count")] public long Count { get; set; }
        [J("total")] public long Total { get; set; }
    }

    public partial class Extensions
    {
        [J("cacheControl")] public CacheControl CacheControl { get; set; }
    }

    public partial class CacheControl
    {
        [J("version")] public long Version { get; set; }
        [J("hints")] public List<Hint> Hints { get; set; }
    }

    public partial class Hint
    {
        [J("path")] public List<Path> Path { get; set; }
        [J("maxAge")] public long MaxAge { get; set; }
    }

    public partial struct Path
    {
        public long? Integer;
        public string String;

        public static implicit operator Path(long Integer) => new Path {Integer = Integer};
        public static implicit operator Path(string String) => new Path {String = String};
    }

    public partial class GraphData
    {
        public static GraphData FromJson(string json) => JsonConvert.DeserializeObject<GraphData>(json,
            Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this GraphData self) => JsonConvert.SerializeObject(self,
            Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                PathConverter.Singleton,
                new IsoDateTimeConverter {DateTimeStyles = DateTimeStyles.AssumeUniversal}
            },
        };
    }

    internal class PathConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Path) || t == typeof(Path?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                    var integerValue = serializer.Deserialize<long>(reader);
                    return new Path {Integer = integerValue};
                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    return new Path {String = stringValue};
            }

            throw new Exception("Cannot unmarshal type Path");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (Path) untypedValue;
            if (value.Integer != null)
            {
                serializer.Serialize(writer, value.Integer.Value);
                return;
            }

            if (value.String != null)
            {
                serializer.Serialize(writer, value.String);
                return;
            }

            throw new Exception("Cannot marshal type Path");
        }

        public static readonly PathConverter Singleton = new PathConverter();
    }
}
