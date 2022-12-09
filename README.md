# SmartSolrSchema

This module enhances the built-in Sitecore Solr Populate Managed Schema functionality by populating not only standard Sitecore dynamic fields but also reads any custom languages that are set up in the Sitecore Master database under `/sitecore/system/languages`.

## Why?

If you add a new language to Sitecore, for example Korean, and start adding items based on that language, when it comes to indexing on Solr you will see issues about missing dynamic fields about the `ko` language.

Example:

> org.apache.solr.common.SolrException: ERROR: [doc=sitecore://master/{bfde3d21-3a67-4938-aa90-33da9caf7bf5}?lang=cs-cz&ver=1] unknown field '__display_name_t_cs' 

Usually you would then need to manually patch the Solr schema yourself [like this](https://sitecore.stackexchange.com/a/2042/1278).

That is a very manual process and makes the DevOps side of things much more tricky. So let's automate it!

## Sitecore Version Support

* Sitecore 7.x - Not Supported
* Sitecore 8.x - Not Supported
* Sitecore 9.0.x - Not Supported
* Sitecore 9.1.x - Supported - Use the [SmartSolrSchema.SC91-100](https://www.nuget.org/packages/SmartSolrSchema.SC91-100) nuget package
* Sitecore 9.2.x - Supported - Use the [SmartSolrSchema.SC91-100](https://www.nuget.org/packages/SmartSolrSchema.SC91-100) nuget package
* Sitecore 9.3.x - Supported - Use the [SmartSolrSchema.SC91-100](https://www.nuget.org/packages/SmartSolrSchema.SC91-100) nuget package
* Sitecore 10.0.x - Supported - Use the [SmartSolrSchema.SC91-100](https://www.nuget.org/packages/SmartSolrSchema.SC91-100) nuget package
* Sitecore 10.1.x - Supported - Use the [SmartSolrSchema.SC101](https://www.nuget.org/packages/SmartSolrSchema.SC101) nuget package
* Sitecore 10.2.x - Supported - Use the [SmartSolrSchema.SC101](https://www.nuget.org/packages/SmartSolrSchema.SC101) nuget package
* Sitecore 10.3.x - Supported - Use the [SmartSolrSchema.SC101](https://www.nuget.org/packages/SmartSolrSchema.SC101) nuget package

## How to install

1. Install the nuget package.
2. If you're using PackageReferences then you'll also need to copy in the configs in the package `App_Config/Modules/SmartSolrSchema`
3. Build your solution and ensure the `SmartSolrSchema.dll` and configs are deployed

## How to use

1. Log into Sitecore and go to the Control Panel
2. Open the Populate Solr Managed Schema window
3. Check the indexes you want to populate and click Populate
4. Ensure that the process Succeeded. You can also check the logs to see what custom languages have been populated. For example: `SmartSolrSchema: adding custom defined language to schema ko`
5. Close the dialog and open the Indexing Manager
6. Rebuild the indexes
7. You can check the Crawling log file to make sure you're not getting errors about unknown fields.

## Date *_dtm Bugfix

This module also fixes a Sitecore bug where Solr datetimeCollection has fieldNameFormat="{0}_dtm" but the schema builder format is "*_tdtm"

## Date Sorting

There is a somewhat common issue with sorting by date fields in Solr where it does not always sort correctly. It happens more often if you have a lot of items that you are searching and ordering.
This has been blogged about [here](https://www.sitecorenutsbolts.net/2014/11/06/Sitecore-Sorting-by-Date-with-SOLR/) by Rich Seal, where the fix is to add a date field to the solr managed schema that is `indexed="true" stored="false"`.
I have made this easier for you by automatically adding a `*_tdts` dynamic field with this setting. You can then use it in your project as follows:

1. Open the `App_Config/Modules/SmartSolrSchema/DateTimeSortable.config` and uncomment the example computed field. Update the `fieldName` parameter with your field name.
2. Add as many fields as you need to sort by here.
3. Build and deploy your solution and do a full index rebuild
4. You can now sort by `fieldname_tdts` which will return accurate results.

## Who

This module is written and maintained by [Mark Gibbons](https://github.com/markgibbons25) and other people in the Dataweavers team.