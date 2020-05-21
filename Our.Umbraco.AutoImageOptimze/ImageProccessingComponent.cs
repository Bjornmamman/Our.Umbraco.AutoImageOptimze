using ImageProcessor.Web.Helpers;
using ImageProcessor.Web.HttpModules;
using StackExchange.Profiling.Internal;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;

namespace Our.Umbraco.AutoImageOptimze
{
	public class ImageProccessingComponent : global::Umbraco.Core.Composing.IComponent
	{
		private IEnumerable<string> _ignore = new List<string>() { ".gif" };

		private bool _defaultWebp = true;
		private int _cacheTimeout = 500;
		private bool _automatic = true;

		private string _defaultQuality = "0";
		private string _defaultMaxWidth = "0";
		private string _defaultMaxHeight = "0";

		private readonly AppCaches _caches;

		public ImageProccessingComponent(AppCaches caches)
		{
			_caches = caches;
		}

		public void Initialize()
		{
			var ignore = ConfigurationManager.AppSettings["Our.Umbraco.AutoImageOptimze:Ignore"];

			
			_defaultWebp = bool.Parse(ConfigurationManager.AppSettings["Our.Umbraco.AutoImageOptimze:WebP"] ?? "true");
			_defaultQuality = ConfigurationManager.AppSettings["Our.Umbraco.AutoImageOptimze:Quality"] ?? "0";
			_defaultMaxWidth = ConfigurationManager.AppSettings["Our.Umbraco.AutoImageOptimze:MaxWidth"] ?? "0";
			_defaultMaxHeight = ConfigurationManager.AppSettings["Our.Umbraco.AutoImageOptimze:MaxHeight"] ?? "0";
			_defaultMaxHeight = ConfigurationManager.AppSettings["Our.Umbraco.AutoImageOptimze:MaxHeight"] ?? "0";

			_automatic = bool.Parse(ConfigurationManager.AppSettings["Our.Umbraco.AutoImageOptimze:Automatic"] ?? "true");

			_cacheTimeout = int.Parse(ConfigurationManager.AppSettings["Our.Umbraco.AutoImageOptimze:CacheTimeout"] ?? "300");

			if (!ignore.IsNullOrWhiteSpace())
				_ignore = ignore.Split(',', ';').Select(x => x.Trim().ToLower());

			ImageProcessingModule.ValidatingRequest += ImageProcessingModule_ValidatingRequest;
		}

		private void ImageProcessingModule_ValidatingRequest(object sender, ValidatingRequestEventArgs e)
		{
			//Disable if url contains "optimize=false"
			if (e.Context.Request.QueryString["optimize"] == "false")
				return;

			//If automatic is sett to false, check for "optimize=true" in reuqest
			if (!_automatic && e.Context.Request.QueryString["optimize"] != "true")
				return;

			//Check if browser supports webp
			var acceptWebP = e.Context.Request.AcceptTypes != null && e.Context.Request.AcceptTypes.Contains("image/webp");

			//Caches the reuqest if CacheTimeout is set
			var modifiedQuery = _cacheTimeout > 0 ? 
				Current.AppCaches.RuntimeCache.GetCacheItem($"Our.Umbraco.AutoImageOptimze:Path{(acceptWebP ? ":WebP" : "")}:{e.Context.Request.Url.PathAndQuery}", () => { return QueryModifier(e); }, TimeSpan.FromSeconds(_cacheTimeout)) 
			: 
				QueryModifier(e);

			//If nothing is changed return
			if (modifiedQuery.IsNullOrWhiteSpace())
				return;


			e.QueryString = modifiedQuery;

		}

		private string QueryModifier(ValidatingRequestEventArgs e)
		{
			var path = e.Context.Request.Url.AbsolutePath;

			if (!_ignore.Any(x => path.Contains(x, StringComparison.InvariantCultureIgnoreCase)))
			{
				//Gets the imageprocessor quertystring
				var queryString = HttpUtility.ParseQueryString(e.QueryString);

				if (_defaultWebp && queryString.Get("format") != "webp" && e.Context.Request.AcceptTypes != null && e.Context.Request.AcceptTypes.Contains("image/webp"))
				{
					queryString.Remove("format");
					queryString["format"] = "webp";
				}


				if (_defaultQuality != "0" && queryString.Get("quality") == null)
				{
					queryString["quality"] = _defaultQuality;
				}

				var maxSize = false;

				if (_defaultMaxWidth != "0" && queryString.Get("width") == null)
				{
					queryString["width"] = _defaultMaxWidth;
					maxSize = true;
				}

				if (_defaultMaxHeight != "0" && queryString.Get("height") == null)
				{
					queryString["height"] = _defaultMaxHeight;
					maxSize = true;
				}

				//If maxsize is set and mode is missing set to max
				if (maxSize == true && queryString.Get("mode") == null)
				{
					queryString["mode"] = "max";
				}

				return queryString.ToString();
			}

			return null;
		}

		public void Terminate()
		{
		}
	}
}
