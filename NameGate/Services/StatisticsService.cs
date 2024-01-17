
using DNS.Server;
using Microsoft.EntityFrameworkCore;
using NameGate.Database;
using NameGate.Database.Entities;
using NameGate.Database.Repositories;
using System.Collections.Generic;
using System.Net;

namespace NameGate.Services
{
    public class StatisticsService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DnsServer _server;
        private readonly ILogger<StatisticsService> _log;

        private readonly Dictionary<string, IpStatisticsEntity> _ipStatistics = new();
        private readonly Dictionary<string, DomainStatisticsEntity> _domainStatistics = new();


        public StatisticsService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _server = serviceProvider.GetRequiredService<DnsServer>();
            _log = serviceProvider.GetRequiredService<ILogger<StatisticsService>>();

            _server.Responded += Server_Responded;
        }

        public async IAsyncEnumerable<IpStatisticsEntity> GetIpStatistics()
        {
            using var scope = _serviceProvider.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IIpStatisticsRepository>();
            var dbStats = await repo.Get().OrderByDescending(x => x.BlockedCount).ThenByDescending(x => x.AllowedCount).Take(100).ToListAsync();

            List<string> ips;

            lock(_ipStatistics)
            {
                ips = _ipStatistics.Values.Select(x => x.IpAddress).Concat(dbStats.Select(x => x.IpAddress)).Distinct().ToList();
            }


            foreach(var ip in ips)
            {
                var ret = new IpStatisticsEntity() { IpAddress = ip };

                var dbStat = dbStats.SingleOrDefault(x => x.IpAddress == ip);
                if(dbStat != null)
                {
                    ret.AllowedCount += dbStat.AllowedCount;
                    ret.BlockedCount += dbStat.BlockedCount;
                }

                lock (_ipStatistics)
                {
                    if(_ipStatistics.TryGetValue(ip, out var memStat))
                    {
                        ret.AllowedCount += memStat.AllowedCount;
                        ret.BlockedCount += memStat.BlockedCount;
                    }
                }

                yield return ret;
            }
        }

        public async IAsyncEnumerable<DomainStatisticsEntity> GetDomainStatistics()
        {
            using var scope = _serviceProvider.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IDomainStatisticsRepository>();
            var dbStats = await repo.Get().OrderByDescending(x => x.BlockedCount).ThenByDescending(x => x.AllowedCount).Take(100).ToListAsync();

            List<string> domains;

            lock (_domainStatistics)
            {
                domains = _domainStatistics.Values.Select(x => x.Domain).Concat(dbStats.Select(x => x.Domain)).Distinct().ToList();
            }


            foreach (var domain in domains)
            {
                var ret = new DomainStatisticsEntity() { Domain = domain };

                var dbStat = dbStats.SingleOrDefault(x => x.Domain == domain);
                if (dbStat != null)
                {
                    ret.AllowedCount += dbStat.AllowedCount;
                    ret.BlockedCount += dbStat.BlockedCount;
                }

                lock (_domainStatistics)
                {
                    if (_domainStatistics.TryGetValue(domain, out var memStat))
                    {
                        ret.AllowedCount += memStat.AllowedCount;
                        ret.BlockedCount += memStat.BlockedCount;
                    }
                }

                yield return ret;
            }
        }

        public override void Dispose()
        {
            _server.Responded -= Server_Responded;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var ipStatsRepo = scope.ServiceProvider.GetRequiredService<IIpStatisticsRepository>();
                var domainStatsRepo = scope.ServiceProvider.GetRequiredService<IDomainStatisticsRepository>();

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // Normal
                    }


                    List<IpStatisticsEntity> ipStats;
                    lock (_ipStatistics)
                    {
                        ipStats = _ipStatistics.Values.ToList();
                        _ipStatistics.Clear();
                    }

                    await ipStatsRepo.Increment(ipStats);

                    _log.LogTrace("Saved IP statistics to db.");


                    List<DomainStatisticsEntity> domainStats;
                    lock (_domainStatistics)
                    {
                        domainStats = _domainStatistics.Values.ToList();
                        _domainStatistics.Clear();
                    }

                    await domainStatsRepo.Increment(domainStats);

                    _log.LogTrace("Saved domain statistics to db.");
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error in statistics routine.");
            }
        }

        private void WithIpStatistic(IPAddress address, Action<IpStatisticsEntity> doWith)
        {
            IpStatisticsEntity? entity;

            lock (_ipStatistics)
            {
                if (!_ipStatistics.TryGetValue(address.ToString(), out entity))
                {
                    entity = new()
                    {
                        IpAddress = address.ToString()
                    };

                    _ipStatistics.Add(entity.IpAddress, entity);
                }
            }

            if (entity == null)
            {
                _log.LogWarning("Unable to fetch IP stats.");
                return;
            }

            doWith(entity);
        }

        private void WithDomainStatistic(string domain, Action<DomainStatisticsEntity> doWith)
        {
            DomainStatisticsEntity? entity;

            lock (_domainStatistics)
            {
                if (!_domainStatistics.TryGetValue(domain, out entity))
                {
                    entity = new()
                    {
                        Domain = domain
                    };

                    _domainStatistics.Add(domain, entity);
                }
            }

            if (entity == null)
            {
                _log.LogWarning("Unable to fetch domain stats.");
                return;
            }

            doWith(entity);
        }

        private void Server_Responded(object? sender, DnsServer.RespondedEventArgs e)
        {
            var wasBlocked = (e.Response as WrappedResponse)?.WasBlocked ?? false;

            var address = e.Remote.Address;
            var domains = e.Response.Questions.Select(x => x.Name.ToString()).Distinct().ToList();

            if (wasBlocked)
            {
                WithIpStatistic(e.Remote.Address, x => x.BlockedCount++);
                foreach(var domain in domains)
                {
                    WithDomainStatistic(domain, x => x.BlockedCount++);
                }
            }
            else
            {
                WithIpStatistic(e.Remote.Address, x => x.AllowedCount++);
                foreach (var domain in domains)
                {
                    WithDomainStatistic(domain, x => x.AllowedCount++);
                }
            }
        }
    }
}
