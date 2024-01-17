using Microsoft.Extensions.Logging.Abstractions;
using NameGate.BlockLists;

namespace NameGate.Tests
{
    public class StevenBlackTests
    {
        private const string testList = """
# Title: StevenBlack/hosts
#
# This hosts file is a merged collection of hosts from reputable sources,
# with a dash of crowd sourcing via GitHub
#
# Date: 18 January 2024 01:06:23 (UTC)
# Number of unique domains: 153,663
#
# Fetch the latest version of this file: https://raw.githubusercontent.com/StevenBlack/hosts/master/hosts
# Project home page: https://github.com/StevenBlack/hosts
# Project releases: https://github.com/StevenBlack/hosts/releases
#
# ===============================================================

127.0.0.1 localhost
127.0.0.1 localhost.localdomain
127.0.0.1 local
255.255.255.255 broadcasthost
::1 localhost
::1 ip6-localhost
::1 ip6-loopback
fe80::1%lo0 localhost
ff00::0 ip6-localnet
ff00::0 ip6-mcastprefix
ff02::1 ip6-allnodes
ff02::2 ip6-allrouters
ff02::3 ip6-allhosts
0.0.0.0 0.0.0.0

# Custom host records are listed here.


# End of custom host records.
# Start StevenBlack

#=====================================
# Title: Hosts contributed by Steven Black
# http://stevenblack.com

0.0.0.0 ck.getcookiestxt.com
0.0.0.0 eu1.clevertap-prod.com
0.0.0.0 wizhumpgyros.com
0.0.0.0 coccyxwickimp.com
0.0.0.0 webmail-who-int.000webhostapp.com
0.0.0.0 010sec.com
0.0.0.0 01mspmd5yalky8.com
0.0.0.0 0byv9mgbn0.com
0.0.0.0 ns6.0pendns.org
0.0.0.0 dns.0pengl.com
0.0.0.0 12724.xyz
0.0.0.0 21736.xyz
0.0.0.0 www.analytics.247sports.com
0.0.0.0 2no.co
0.0.0.0 www.2no.co
0.0.0.0 logitechlogitechglobal.112.2o7.net
0.0.0.0 www.logitechlogitechglobal.112.2o7.net
0.0.0.0 2s11.com
0.0.0.0 30-day-change.com
0.0.0.0 www.30-day-change.com
0.0.0.0 mclean.f.360.cn
0.0.0.0 mvconf.f.360.cn
0.0.0.0 care.help.360.cn
0.0.0.0 eul.s.360.cn
0.0.0.0 g.s.360.cn
0.0.0.0 p.s.360.cn
0.0.0.0 aicleaner.shouji.360.cn
0.0.0.0 ssl.360antivirus.org
0.0.0.0 ad.360in.com
0.0.0.0 mclean.lato.cloud.360safe.com
0.0.0.0 mvconf.lato.cloud.360safe.com
0.0.0.0 mclean.cloud.360safe.com
0.0.0.0 mvconf.cloud.360safe.com
0.0.0.0 mclean.uk.cloud.360safe.com
0.0.0.0 mvconf.uk.cloud.360safe.com
0.0.0.0 3lift.org
0.0.0.0 448ff4fcfcd199a.com
0.0.0.0 44chan.me
0.0.0.0 4ourkidsky.com
0.0.0.0 5kv261gjmq04c9.com
0.0.0.0 88chan.pw
0.0.0.0 new.915yzt.cn
0.0.0.0 tempinfo.96.lt
0.0.0.0 abdurantom.com
0.0.0.0 abtasty.net
0.0.0.0 analytics.modul.ac.at
0.0.0.0 acalvet.com
0.0.0.0 acbras.com
0.0.0.0 graph.accountkit.com
0.0.0.0 www.graph.accountkit.com
0.0.0.0 go.ad1data.com
0.0.0.0 metrics.adage.com
0.0.0.0 adaptivecss.org
0.0.0.0 ads30.adcolony.com
0.0.0.0 androidads23.adcolony.com
0.0.0.0 events3.adcolony.com
0.0.0.0 events3alt.adcolony.com
0.0.0.0 sdk.adincube.com
0.0.0.0 app.adjust.com
0.0.0.0 cdn.admitad-connect.com
0.0.0.0 macro.adnami.io
0.0.0.0 acdn.adnxs.com
0.0.0.0 prebid.adnxs.com
0.0.0.0 www.prebid.adnxs.com
0.0.0.0 sstats.adobe.com
0.0.0.0 adorebeauty.org
0.0.0.0 feedback.adrecover.com
0.0.0.0 adroitpmps.com
0.0.0.0 www.ads2live.com
0.0.0.0 adsflame.com
0.0.0.0 backend-ssp.adstudio.cloud
0.0.0.0 adtekmedia.com
0.0.0.0 400.route.to.adtracker.network
0.0.0.0 track.adtrue.com
0.0.0.0 dw.adyboh.com
0.0.0.0 wy.adyboh.com
0.0.0.0 adzerk.com
0.0.0.0 www.adzerk.com
0.0.0.0 ark.aeriagames.us
0.0.0.0 ahf4ycvea439tt9rq.site
0.0.0.0 aiisa.am
0.0.0.0 aircovid19virus.com
0.0.0.0 akashdayalgroups.com
0.0.0.0 www.akashdayalgroups.com
0.0.0.0 akibaol.com
0.0.0.0 alidnx.com
0.0.0.0 alliance4media.com
0.0.0.0 www.alliance4media.com
0.0.0.0 allmygoodlife.com
0.0.0.0 acrdb.alphonso.tv
0.0.0.0 ads.alphonso.tv
0.0.0.0 api.alphonso.tv
0.0.0.0 assets.alphonso.tv
0.0.0.0 bwlkup.alphonso.tv
0.0.0.0 clockskew.alphonso.tv
0.0.0.0 degea.alphonso.tv
0.0.0.0 demo.alphonso.tv
0.0.0.0 dimaria.alphonso.tv
0.0.0.0 insights.alphonso.tv
0.0.0.0 koudas.alphonso.tv
0.0.0.0 neymar.alphonso.tv
0.0.0.0 optout.alphonso.tv
0.0.0.0 ppjs.alphonso.tv
0.0.0.0 prov.alphonso.tv
0.0.0.0 proxy.alphonso.tv
0.0.0.0 rtbdev4.alphonso.tv
0.0.0.0 tn.alphonso.tv
0.0.0.0 tr.alphonso.tv
0.0.0.0 tvads.alphonso.tv
0.0.0.0 villa.alphonso.tv
0.0.0.0 web2.alphonso.tv
0.0.0.0 www.alphonso.tv
0.0.0.0 alpinehandlingsystems.com
0.0.0.0 creative.alxbgo.com
0.0.0.0 amarktflow.com
0.0.0.0 www.amarktflow.com
0.0.0.0 device-metrics-us.amazon.com
0.0.0.0 device-metrics-us-2.amazon.com
0.0.0.0 windowsphishingalert158.s3.amazonaws.com
0.0.0.0 amazonco.uk
0.0.0.0 www.amazonco.uk
0.0.0.0 amorluv.com
0.0.0.0 analytics-digit-in.cdn.ampproject.org
0.0.0.0 www.analytics-digit-in.cdn.ampproject.org
0.0.0.0 annualconsumersurvey.com
0.0.0.0 www.annualconsumersurvey.com
0.0.0.0 antivirus-covid19.site
0.0.0.0 aoredi.com
0.0.0.0 ap-ms.net
0.0.0.0 jhqvy.app.link
0.0.0.0 api.stats.appa.pe
0.0.0.0 dev.appboy.com
0.0.0.0 orion.iad.appboy.com
0.0.0.0 www.orion.iad.appboy.com
0.0.0.0 adsaccount.appcpi.net
0.0.0.0 cloud-based-service.us-south.cf.appdomain.cloud
0.0.0.0 app.appleadwords.net
0.0.0.0 www.applicationunificationcontroller.com
0.0.0.0 d.applovin.com
0.0.0.0 rt.applovin.com
0.0.0.0 events.appsflyer.com
0.0.0.0 register.appsflyer.com
0.0.0.0 t.appsflyer.com
0.0.0.0 as54rdxfzxs.appspot.com
0.0.0.0 asgh65tfsdxz.appspot.com
0.0.0.0 da032opzasz.appspot.com
0.0.0.0 dgyu536ds.appspot.com
0.0.0.0 eqit9pzsxz.appspot.com
0.0.0.0 gdh4szx.appspot.com
0.0.0.0 hg76ytsdas.appspot.com
0.0.0.0 hj67fadszx.appspot.com
0.0.0.0 hk567rsda.appspot.com
0.0.0.0 hksdf924pzxoias.appspot.com
0.0.0.0 iwe8pzosa.appspot.com
0.0.0.0 k87yfgsdaa.appspot.com
0.0.0.0 kga9szxosa.appspot.com
0.0.0.0 kj65rdasz.appspot.com
0.0.0.0 kj6787rsd.appspot.com
0.0.0.0 kr9apzxosa.appspot.com
0.0.0.0 nffdg43zx.appspot.com
0.0.0.0 oi8ytfzxa.appspot.com
0.0.0.0 ruw82qpzxas.appspot.com
0.0.0.0 tir94wepsdxox.appspot.com
0.0.0.0 tr54sdsazxas.appspot.com
0.0.0.0 tru465rsda.appspot.com
0.0.0.0 uy054eprsdoz.appspot.com
0.0.0.0 xasf32easzx.appspot.com
0.0.0.0 xoada0pzosa.appspot.com
0.0.0.0 ytuy45fxs.appspot.com
0.0.0.0 yu56tdfcxc.appspot.com
0.0.0.0 yuhfdwesaa.appspot.com
0.0.0.0 ad.apsalar.com
0.0.0.0 e-ssl.apsalar.com
0.0.0.0 analytics.archive.org
0.0.0.0 aresboot.xyz
0.0.0.0 arjoflor.com
0.0.0.0 arjoflos.com
0.0.0.0 arkonziv.com
0.0.0.0 www.arkonziv.com
0.0.0.0 armantark.com
0.0.0.0 armconsul.ru
0.0.0.0 artbbs.to
0.0.0.0 artizaa.com
0.0.0.0 cdn.auditude.com
0.0.0.0 auth-google.site
0.0.0.0 analytics.ff.avast.com
0.0.0.0 api.c.avazunativeads.com
0.0.0.0 aws-update.net
0.0.0.0 adv.axiatraders.com
0.0.0.0 www.adv.axiatraders.com
0.0.0.0 api.axiatraders.com
0.0.0.0 www.api.axiatraders.com
0.0.0.0 bid.axiatraders.com
0.0.0.0 www.bid.axiatraders.com
0.0.0.0 click.axiatraders.com
0.0.0.0 www.click.axiatraders.com
0.0.0.0 cvent.axiatraders.com
0.0.0.0 www.cvent.axiatraders.com
0.0.0.0 pub.axiatraders.com
0.0.0.0 www.pub.axiatraders.com
0.0.0.0 rotator.axiatraders.com
0.0.0.0 www.rotator.axiatraders.com
0.0.0.0 www.axiatraders.com
0.0.0.0 zamora.axiatraders.com
0.0.0.0 www.zamora.axiatraders.com
0.0.0.0 api.b2c.com
0.0.0.0 an.batmobi.net
0.0.0.0 gtsdk.batmobi.net
0.0.0.0 ploy.batmobi.net
0.0.0.0 api5.batmobil.net
0.0.0.0 beauty-tea.com
0.0.0.0 www.beauty-tea.com
0.0.0.0 beeglivesex.com
0.0.0.0 cdn.beginads.com
0.0.0.0 www.cdn.beginads.com
0.0.0.0 rotator.beginads.com
0.0.0.0 www.rotator.beginads.com
0.0.0.0 tracking.beginads.com
0.0.0.0 www.tracking.beginads.com
0.0.0.0 www.beginads.com
0.0.0.0 bergadimspower.com
0.0.0.0 berkesteermaster.com
0.0.0.0 www.best2017games.com
0.0.0.0 best2019-games-web4.com
0.0.0.0 bestprizesday3.life
0.0.0.0 besttus.com
0.0.0.0 de.betano.com
0.0.0.0 betpromo247.com
0.0.0.0 www.betpromo247.com
0.0.0.0 bidgear.com
0.0.0.0 platform.bidgear.com
0.0.0.0 www.bidgear.com
0.0.0.0 bigsharkmedia.com
0.0.0.0 bigtus.com
0.0.0.0 assets.bilsyndication.com
0.0.0.0 biltag.bilsyndication.com
0.0.0.0 services.bilsyndication.com
0.0.0.0 cdn.bitmedianetwork.com
0.0.0.0 engine.bitmedianetwork.com
0.0.0.0 bitstarz1.eu
0.0.0.0 bitstarz27.com
0.0.0.0 bitstarz28.com
0.0.0.0 blasze.com
0.0.0.0 blasze.tk
0.0.0.0 bmwforum.co
0.0.0.0 braincdn.org
0.0.0.0 hb.brainlyads.com
0.0.0.0 report.hb.brainlyads.com
0.0.0.0 brainschampions.com
0.0.0.0 client-analytics.braintreegateway.com
0.0.0.0 api.branch.io
0.0.0.0 brilns.com
0.0.0.0 brotherselectricco.com
0.0.0.0 bs.direct
0.0.0.0 analyticsapi.bsbportal.com
0.0.0.0 mi.btmods.net
0.0.0.0 us.btmods.net
0.0.0.0 bunnyland.cc
0.0.0.0 busyserviceinc.com
0.0.0.0 buzzclicks.com
0.0.0.0 promo.buzzclicks.com
0.0.0.0 www.promo.buzzclicks.com
0.0.0.0 rotator.buzzclicks.com
0.0.0.0 www.rotator.buzzclicks.com
0.0.0.0 www.buzzclicks.com
0.0.0.0 www.bywebsite.com
0.0.0.0 applog.camera360.com
""";


        [Fact]
        public async Task TestBlockList()
        {
            var bl = GetBlockList();
            await bl.ForceLoadList(new StringReader(testList));

            await TestBlockListInternal(bl);
        }

        [Fact]
        public async Task TestBlockListDownload()
        {
            var bl = GetBlockList();
            await bl.RefreshCachedList();

            await TestBlockListInternal(bl);
        }


        private StevenBlackBlockList GetBlockList()
        {
            var bl = new StevenBlackBlockList(Services.GetServiceProvider(), new());
            return bl;
        }


        private async Task TestBlockListInternal(IDomainBlockList bl)
        {
            Assert.True(await bl.DomainIsBlocked("2s11.com"));
            Assert.False(await bl.DomainIsBlocked("test.2s11.com"));
            Assert.False(await bl.DomainIsBlocked("a2s11.com"));
        }
    }
}