# Allows automatic imageoptimaztion with Imageprocessor.

## This package includes ImageProcessor.Plugins.WebP 1.3 and requires ImageProcessor (>= 2.8.0)

The plugin will "optimize" by adding default variables to imageprocessor.web querystring

Appsettings

<!-- Disables the plugin -->
<add key="Our.Umbraco.AutoImageOptimze:Disabled" value="false" />

<!-- Enabled automatic webp plugin -->
<add key="Our.Umbraco.AutoImageOptimze:WebP" value="true" />

<!-- Ignores extensions and paths - Example: "app_plugins, .gif" will ignore app_plugins folder and .gif -->
<add key="Our.Umbraco.AutoImageOptimze:Ignore" value=""/>

<!-- Sets default quality -->
<add key="Our.Umbraco.AutoImageOptimze:Quality" value="0" />

<!-- Set maxwidth for all none cropped images -->
<add key="Our.Umbraco.AutoImageOptimze:MaxWidth" value="0" />

<!-- Set maxheight for all none cropped images  -->
<add key="Our.Umbraco.AutoImageOptimze:MaxHeight" value="0" />

<!-- Caches all requests for 5 minutes, 0 will disable cache -->
<add key="Our.Umbraco.AutoImageOptimze:CacheTimeout" value="300" />

<!-- Reverts default logic, and will not "optimize" unless "optimize=true" is set in the querystring -->
<add key="Our.Umbraco.AutoImageOptimze:Automatic" value="false" />
