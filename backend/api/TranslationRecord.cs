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

    public class TranslationRequest
    {
        [JsonProperty(PropertyName = "lang_in")]
        public string LangIn { get; set; }

        [JsonProperty(PropertyName = "lang_out")]
        public string LangOut { get; set; }

        [JsonProperty(PropertyName = "text_in")]
        public string TextIn { get; set; }

        public TranslationRequest(string lang_in, string lang_out, string text_in){
            this.TextIn = text_in;
            this.LangIn = lang_in;
            this.LangOut = lang_out;
        }
    }


}