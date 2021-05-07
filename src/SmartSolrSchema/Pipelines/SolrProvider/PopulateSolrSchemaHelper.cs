using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Sitecore.Configuration;
using Sitecore.ContentSearch.SolrProvider.Pipelines.PopulateSolrSchema;
using Sitecore.Diagnostics;
using SolrNet.Schema;

namespace SmartSolrSchema.Pipelines.SolrProvider
{
    public class PopulateSolrSchemaHelper : ISchemaPopulateHelper
    {
        private readonly SolrSchema solrSchema;

        public PopulateSolrSchemaHelper(SolrSchema solrSchema)
        {
            Assert.ArgumentNotNull(solrSchema, "solrSchema");
            this.solrSchema = solrSchema;
        }

        public virtual IEnumerable<XElement> GetAllFields()
        {
            var addFields = GetAddFields().Where(x => x != null).ToList();
            var langSpecific = GetAddFieldsLangSpecific(addFields);
            return from o in GetRemoveFields().Union(addFields).Union(langSpecific)
                   where o != null
                   select o;
        }

        public virtual IEnumerable<XElement> GetAllFieldTypes()
        {
            return from o in GetReplaceFields().Union(GetAddFieldTypes())
                   where o != null
                   select o;
        }

        protected virtual bool TypeExists(string type)
        {
            return solrSchema.FindSolrFieldTypeByName(type) != null;
        }

        protected XElement CreateField(string name, string type, bool required, bool indexed, bool stored, bool multiValued, bool omitNorms, bool termVectors, bool termPositions, bool termOffsets, string defaultValue = null, bool isDynamic = false)
        {
            if (!TypeExists(type))
            {
                return null;
            }

            string expandedName = isDynamic ? "add-dynamic-field" : "add-field";
            XElement xElement = new XElement(expandedName);
            xElement.Add(new XElement("name", name));
            xElement.Add(new XElement("type", type));
            xElement.Add(new XElement("indexed", indexed.ToString().ToLowerInvariant()));
            xElement.Add(new XElement("stored", stored.ToString().ToLowerInvariant()));
            if (required)
            {
                xElement.Add(new XElement("required", true));
            }

            if (multiValued)
            {
                xElement.Add(new XElement("multiValued", true));
            }

            if (omitNorms)
            {
                xElement.Add(new XElement("omitNorms", true));
            }

            if (termVectors)
            {
                xElement.Add(new XElement("termVectors", true));
            }

            if (termPositions)
            {
                xElement.Add(new XElement("termPositions", true));
            }

            if (termOffsets)
            {
                xElement.Add(new XElement("termOffsets", true));
            }

            if (!string.IsNullOrEmpty(defaultValue))
            {
                xElement.Add(new XElement("default", defaultValue));
            }

            return xElement;
        }

        protected XElement CreateFieldType(string name, string @class, IDictionary<string, string> properties)
        {
            XElement xElement = new XElement(TypeExists(name) ? "replace-field-type" : "add-field-type");
            xElement.Add(new XElement("name", name));
            xElement.Add(new XElement("class", @class));
            foreach (KeyValuePair<string, string> property in properties)
            {
                xElement.Add(new XElement(property.Key, property.Value));
            }

            return xElement;
        }

        private static XElement GetRemoveField(string name, bool isDynamicField = false)
        {
            Assert.ArgumentNotNull(name, "name");
            XElement xElement = new XElement(isDynamicField ? "delete-dynamic-field" : "delete-field");
            xElement.Add(new XElement("name", name));
            return xElement;
        }

        private static XElement GetRemoveCopyField(string source, string destination)
        {
            Assert.ArgumentNotNull(source, "source");
            Assert.ArgumentNotNull(destination, "destination");
            XElement xElement = new XElement("delete-copy-field");
            xElement.Add(new XElement("source", source));
            xElement.Add(new XElement("dest", destination));
            return xElement;
        }

        private IEnumerable<XElement> GetRemoveFields()
        {
            foreach (SolrNet.Schema.SolrCopyField solrCopyField in solrSchema.SolrCopyFields)
            {
                yield return GetRemoveCopyField(solrCopyField.Source, solrCopyField.Destination);
            }

            foreach (SolrDynamicField solrDynamicField in solrSchema.SolrDynamicFields)
            {
                yield return GetRemoveField(solrDynamicField.Name, isDynamicField: true);
            }

            foreach (SolrField solrField in solrSchema.SolrFields)
            {
                yield return GetRemoveField(solrField.Name);
            }
        }

        private IEnumerable<XElement> GetAddFieldTypes()
        {
            yield return CreateFieldType("random", "solr.RandomSortField", new Dictionary<string, string>
            {
                {
                    "indexed",
                    "true"
                }
            });
            yield return CreateFieldType("ignored", "solr.StrField", new Dictionary<string, string>
            {
                {
                    "indexed",
                    "false"
                },
                {
                    "stored",
                    "false"
                },
                {
                    "docValues",
                    "false"
                },
                {
                    "multiValued",
                    "true"
                }
            });
        }
        
        private IEnumerable<XElement> GetAddFields()
        {
            yield return CreateField("_content", "text_general", required: false, indexed: true, stored: false, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_database", "lowercase", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_path", "string", required: false, indexed: true, stored: true, multiValued: true, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_uniqueid", "string", required: true, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_datasource", "lowercase", required: true, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_parent", "string", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_name", "text_general", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_displayname", "text_general", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_language", "string", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_creator", "lowercase", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_editor", "lowercase", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_created", "pdate", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_updated", "pdate", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_hidden", "boolean", required: false, indexed: true, stored: false, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_template", "lowercase", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_templatename", "lowercase", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_templates", "string", required: false, indexed: true, stored: true, multiValued: true, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_icon", "lowercase", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_links", "lowercase", required: false, indexed: true, stored: true, multiValued: true, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_tags", "lowercase", required: false, indexed: true, stored: true, multiValued: true, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_group", "string", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_indexname", "lowercase", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_latestversion", "boolean", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_indextimestamp", "pdate", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, "NOW");
            yield return CreateField("_fullpath", "lowercase", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_isclone", "boolean", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_version", "string", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_hash", "string", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("__semantics", "string", required: false, indexed: true, stored: true, multiValued: true, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("__boost", "pfloat", required: false, indexed: true, stored: true, multiValued: false, omitNorms: true, termVectors: false, termPositions: false, termOffsets: false, "0");
            yield return CreateReadAccessField();
            yield return CreateField("lock", "boolean", required: false, indexed: true, stored: false, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("__bucketable", "boolean", required: false, indexed: true, stored: false, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("__workflow_state", "string", required: false, indexed: true, stored: false, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("__is_bucket", "boolean", required: false, indexed: true, stored: false, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("is_displayed_in_search_results", "boolean", required: false, indexed: true, stored: false, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("text", "text_general", required: false, indexed: true, stored: false, multiValued: true, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("text_rev", "text_general_rev", required: false, indexed: true, stored: false, multiValued: true, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("alphaNameSort", "alphaOnlySort", required: false, indexed: true, stored: false, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("__hidden", "boolean", required: false, indexed: true, stored: false, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("_version_", "plong", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            yield return CreateField("*_t", "text_general", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_en", "text_en", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_ar", "text_ar", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_bg", "text_bg", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_ca", "text_ca", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_cs", "text_cz", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_da", "text_da", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_de", "text_de", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_el", "text_el", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_es", "text_es", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_eu", "text_eu", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_fa", "text_fa", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_fi", "text_fi", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_fr", "text_fr", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_ga", "text_ga", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_gl", "text_gl", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_hi", "text_hi", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_hu", "text_hu", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_hy", "text_hy", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_id", "text_id", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_it", "text_it", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_ja", "text_ja", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_lv", "text_lv", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_nl", "text_nl", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_nb", "text_no", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_pt", "text_pt", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_ro", "text_ro", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_ru", "text_ru", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_sv", "text_sv", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_th", "text_th", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_t_tr", "text_tr", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_i", "pint", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_s", "string", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_sm", "string", required: false, indexed: true, stored: true, multiValued: true, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_ls", "lowercase", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_lsm", "lowercase", required: false, indexed: true, stored: true, multiValued: true, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_im", "pint", required: false, indexed: true, stored: true, multiValued: true, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_txm", "text_general", required: false, indexed: true, stored: true, multiValued: true, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_b", "boolean", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_dt", "pdate", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_p", "location", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_ti", "pint", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_tl", "plong", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_tf", "pfloat", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_td", "pdouble", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_tdt", "pdate", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_tdtm", "pdate", required: false, indexed: true, stored: true, multiValued: true, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_pi", "pint", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_c", "currency", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_ignored", "ignored", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_random", "random", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
            yield return CreateField("*_rpt", "location_rpt", required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
        }

        private List<XElement> GetAddFieldsLangSpecific(List<XElement> addFields)
        {
            var langs = Factory.GetDatabase("master")
                .GetItem("/sitecore/system/Languages")
                .Children
                .Select(x => x["Iso"])
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => $"*_t_{x}")
                .ToList()
                .Distinct();
            var addFieldsLangs = addFields
                .Select(x => x.Element("name")?.Value)
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
            var toAdd = langs
                .Where(x => !addFieldsLangs.Contains(x))
                .ToList();
            var result = new List<XElement>();
            foreach (var fieldname in toAdd)
            {
                var lang = fieldname.Replace("*_t_", "");
                result.Add(CreateDynamicFieldWithFallbackFieldType(fieldname, $"text_{lang}"));
                Sitecore.Diagnostics.Log.Info($"SmartSolrSchema: adding custom defined language to schema {lang}", this);
            }
            return result;
        }

        private XElement CreateReadAccessField()
        {
            XElement xElement = CreateField("_readaccess", "lowercase", required: false, indexed: true, stored: false, multiValued: true, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false);
            xElement.Add(new XElement("docValues", false));
            return xElement;
        }

        private XElement CreateDynamicFieldWithFallbackFieldType(string fieldName, string fieldType, string fallbackFieldType = "text_general")
        {
            Assert.ArgumentNotNull(fieldName, "fieldName");
            Assert.ArgumentNotNull(fieldType, "fieldType");
            string type = TypeExists(fieldType) ? fieldType : fallbackFieldType;
            return CreateField(fieldName, type, required: false, indexed: true, stored: true, multiValued: false, omitNorms: false, termVectors: false, termPositions: false, termOffsets: false, null, isDynamic: true);
        }

        private IEnumerable<XElement> GetReplaceFields()
        {
            yield return GetReplaceTextGeneralFieldType();
        }

        private XElement GetReplaceTextGeneralFieldType()
        {
            SolrFieldType solrFieldType = solrSchema.SolrFieldTypes.Find((SolrFieldType f) => f.Name == "text_general");
            if (solrFieldType == null)
            {
                return null;
            }

            XElement xElement = new XElement("replace-field-type");
            xElement.Add(new XElement("name", solrFieldType.Name));
            xElement.Add(new XElement("class", solrFieldType.Type));
            xElement.Add(new XElement("positionIncrementGap", "100"));
            xElement.Add(new XElement("multiValued", "false"));
            XElement xElement2 = new XElement("indexAnalyzer");
            xElement2.Add(new XElement("tokenizer", new XElement("class", "solr.StandardTokenizerFactory")));
            xElement2.Add(new XElement("filters", new XElement("class", "solr.StopFilterFactory"), new XElement("ignoreCase", "true"), new XElement("words", "stopwords.txt")));
            xElement2.Add(new XElement("filters", new XElement("class", "solr.LowerCaseFilterFactory")));
            xElement.Add(xElement2);
            XElement xElement3 = new XElement("queryAnalyzer");
            xElement3.Add(new XElement("tokenizer", new XElement("class", "solr.StandardTokenizerFactory")));
            xElement3.Add(new XElement("filters", new XElement("class", "solr.StopFilterFactory"), new XElement("ignoreCase", "true"), new XElement("words", "stopwords.txt")));
            xElement3.Add(new XElement("filters", new XElement("class", "solr.SynonymFilterFactory"), new XElement("synonyms", "synonyms.txt"), new XElement("ignoreCase", "true"), new XElement("expand", "true")));
            xElement3.Add(new XElement("filters", new XElement("class", "solr.LowerCaseFilterFactory")));
            xElement.Add(xElement3);
            return xElement;
        }
    }

#if NET48

    public class DefaultPopulateHelperFactory : Sitecore.ContentSearch.SolrProvider.Abstractions.IPopulateHelperFactory
    {
        public ISchemaPopulateHelper GetPopulateHelper(SolrSchema solrSchema) => new PopulateSolrSchemaHelper(solrSchema);
    }

#else

    public class PopulateFields : Sitecore.ContentSearch.SolrProvider.Pipelines.PopulateSolrSchema.PopulateFields
    {
        protected override ISchemaPopulateHelper GetHelper(SolrSchema solrSchema) => new PopulateSolrSchemaHelper(solrSchema);
    }

#endif
}