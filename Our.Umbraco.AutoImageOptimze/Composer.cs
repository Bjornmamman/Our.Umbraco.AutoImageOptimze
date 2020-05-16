using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Our.Umbraco.AutoImageOptimze
{
    public class Composer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            if (!(bool.Parse(ConfigurationManager.AppSettings["Our.Umbraco.AutoImageOptimze:Disabled"] ?? "false")))
                composition.Components().Append<Our.Umbraco.AutoImageOptimze.ImageProccessingComponent>();
        }
    }
}
