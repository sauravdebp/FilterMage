using GoogleAds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FilterMage.Utils
{
    static class Advertisement
    {
        private static InterstitialAd interstitialAd = null;
        public static AdView BannerAd()
        {
            AdView bannerAd = new AdView()
            {
                Format = AdFormats.SmartBanner,
                AdUnitID = "ca-app-pub-5260272565551078/9645418145"
            };
            AdRequest adReq = new AdRequest();
            adReq.ForceTesting = true;
            bannerAd.LoadAd(adReq);
            return bannerAd;
        }

        public static void InterstitialAd()
        {
            Advertisement.interstitialAd = new InterstitialAd("ca-app-pub-5260272565551078/4017686948");
            AdRequest adReq = new AdRequest();
            adReq.ForceTesting = true;
            interstitialAd.LoadAd(adReq);
            interstitialAd.ReceivedAd += (object sender, AdEventArgs e) => 
            {
                MessageBox.Show("Interstitial received");
                interstitialAd.ShowAd();
            };
        }
    }
}
