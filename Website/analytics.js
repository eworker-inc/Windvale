const GOOGLE_ANALYTICS_MEASUREMENT_ID = "G-3PB4LZFMRE";

window.dataLayer = window.dataLayer || [];
window.gtag = window.gtag || function Gtag() {
    window.dataLayer.push(arguments);
};

window.gtag("js", new Date());
window.gtag("config", GOOGLE_ANALYTICS_MEASUREMENT_ID, {
    allow_google_signals: false,
    allow_ad_personalization_signals: false,
});
