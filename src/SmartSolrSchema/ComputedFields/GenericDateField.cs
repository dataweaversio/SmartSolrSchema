using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.Data.Fields;

namespace SmartSolrSchema.ComputedFields
{
    public class GenericDateField : IComputedIndexField
    {
        public string FieldName { get; set; }
        public string ReturnType { get; set; }

        public object ComputeFieldValue(IIndexable indexable)
        {
            var indexItem = indexable as SitecoreIndexableItem;
            if (indexItem?.Item == null) return null;
            var item = indexItem.Item;

            if (string.IsNullOrEmpty(item.Fields[FieldName]?.Value)) return null;

            DateField fieldValue = item.Fields[FieldName];
            if ((fieldValue == null) || (fieldValue.InnerField.Value == null)) return null;

            return fieldValue.DateTime;
        }
    }
}