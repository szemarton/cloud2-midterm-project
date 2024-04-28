using Newtonsoft.Json;

namespace Company.Function
{
    public class TranslationRecord
    {
        [JsonProperty(PropertyName = "_in")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "lang_in")]
        public string LangIn { get; set; }

        [JsonProperty(PropertyName = "lang_out")]
        public string LangOut { get; set; }

        [JsonProperty(PropertyName = "text_in")]
        public string TextIn { get; set; }

        [JsonProperty(PropertyName = "text_out")]
        public string TextOut { get; set; }
    }

}