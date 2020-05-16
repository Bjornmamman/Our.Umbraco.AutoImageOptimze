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

			if (!ignore.IsNullOrWhiteSpace())
				_ignore = ignore.Split(',').Select(x => x.Trim().ToLower());

			ImageProcessingModule.ValidatingRequest += ImageProcessingModule_ValidatingRequest;
		}

		private void ImageProcessingModule_ValidatingRequest(object sender, ValidatingRequestEventArgs e)
		{
			var queryModifier = Current.AppCaches.RuntimeCache.GetCacheItem("Our.Umbraco.AutoImageOptimze:Path:" + e.Context.Request.Url.PathAndQuery, () =>
			{
				var path = e.Context.Request.Url.AbsolutePath;

				if (!_ignore.Any(x => path.Contains(x, StringComparison.InvariantCultureIgnoreCase)))
				{
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

					if (maxSize == true && queryString.Get("mode") == null)
					{
						queryString["mode"] = "max";
					}

					return queryString.ToString();
				}

				return null;
			}, null);

			if (!queryModifier.IsNullOrWhiteSpace())
				e.QueryString = queryModifier;



		}

		public void Terminate()
		{
		}
	}
}
