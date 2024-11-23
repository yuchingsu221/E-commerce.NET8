using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;

namespace YuChingECommerce.Utility
{

    public class ChineseConverterService
    {
        public string ConvertToTraditional(string simplifiedText)
        {
            return ChineseConverter.Convert(simplifiedText, ChineseConversionDirection.SimplifiedToTraditional);
        }

        public string ConvertToSimplified(string traditionalText)
        {
            return ChineseConverter.Convert(traditionalText, ChineseConversionDirection.TraditionalToSimplified);
        }
    }
}
