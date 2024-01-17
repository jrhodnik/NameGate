# 🏷️ NameGate
NameGate is a standalone, independent DNS blocking service similar to [Pi-hole](https://github.com/pi-hole/pi-hole). It supports running on both Linux and Windows. It is extremely easy to setup and deploy, and has a basic web UI that allows for whitelisting of domains, hosts, and exposes other features.

## ⚠️ Disclosure
I wrote this project in a rage-filled frenzy after I couldn't Pi-hole to run in a Proxmox LXC container. It was largely written over the span of a few evenings and it has been running my home network for the last 6 months with no issues. Still, given the nature of the project and it's hasty creation, you probably shouldn't use it in a mission-critical situation.

## 🥧 Versus Pi-hole
What sets NameGate apart is that it doesn't rely on any external services such as FTL DNS, dnsmasq, lighthttpd, etc. All functionality is contained in one single executable, thus it is extremely easy to deploy and maintain.

# 🧠 Features
NameGate is a very simple project. It features a web interface that can do the following:
- Whitelist certain domains, including allowing glob-style wildcards.
  - `someaddomain.com` would only allow queries for that specific domain.
  - `*.someaddomain.com` would allow queries such as `asdf.someaddomain.com`, `jkl.someaddomain.com`, etc.
  - `*someaddomain.com` would allow the above, as well as names like `asdfsomeaddomain.com`.
- Blocklist bypass for certain hosts that shouldn't be subject to domain filtering.
  - By IP, such as `192.168.1.12`, `192.168.1.*`, etc as above.
  - By Hostname, such as `DESKTOP-ASDFJKL`, `DESKTOP-*`, etc as above.
    - Ensure host name lookup is functional using the `ReverseLookupServer` config option detailed below.
- A quick tool to check if a domain is blocked.
  - Enter your domain right in the web UI and it will show if it's blocked or not.
- A simple statistics page to see what kinds of queries are being performed which hosts are making the most queries (blocked or othewise).
  - Track which hosts are making the most blocked queries.
  - Track which blocked domains are being queried the most.
- A live query log, to see what hosts are querying what domains in real time.
  - Useful for diagnostics.
  - Able to see if the request was allowed or blocked.

## 🛑 Block lists
Currently, NameGate utilizes three block lists:
1. The [FireBog](https://v.firebog.net/hosts/lists.php) block list collection, specifically from [this URL](https://v.firebog.net/hosts/csv.txt).
2. The [oisd](https://oisd.nl/) block list, specifically from [this URL](https://big.oisd.nl/domainswild2).
3. The [Steven Black](https://github.com/StevenBlack/hosts) host list, specifically from [this URL](https://raw.githubusercontent.com/StevenBlack/hosts/master/hosts).

Block lists automatically refreshed every 24 hours (configurable), and are cached on disk. 

# ✅ Setup
NameGate is quite simple to deploy. It can be ran standalone or as a service, however you choose. 

## 📝 Configuration and Starting
Download the latest binaries from the releases page, and extract to your desired location.

Optionally modify `appsettings.json`, where you can configure which DNS servers are used to fulfill standard queries using the `DnsServers` array. 

You can also configure `ReverseLookupServer` to a DNS server that is able to perform IP address reverse lookups, ie PTR record lookups. Doing so will show hostnames in the UI, and allow host bypassing by hostname. If left null, we will try with the first in `DnsServers`.

Launch the service using `./NameGate --urls=http://127.0.0.1:80/`.

This will allow you to access the web UI from the localhost. If you want to access it from anywhere you *could* set that IP to `0.0.0.0`, but NameGuard doesn't currently have any sort of user authentication so anyone *could* simply access the web UI if it is publicly exposed.

If you want add security to the NameGate web UI, consider a reverse proxy such as [caddy](https://github.com/caddyserver/caddy) with their [`basic_auth` directive](https://caddyserver.com/docs/caddyfile/directives/basic_auth).

## 🧪 Testing
You can test queries against your NameGate instance using the `nslookup` command.

1. Ensure NameGate is running
2. Open your terminal and run `nslookup`.
3. Enter `server 192.168.1.123`, or whatever the IP of your NameGate server is.
4. Enter a domain to lookup, such as `github.com.`. Notice the `.` at the end of the domain, this is important.
  - This should give a normal result that contains the standard Github domain lookup results.
5. Enter a known blocked domain, such as `ads.youtube.com.`.
  - This should instead give you a `REFUSED` or `Query refused` result.

If all this goes well, you should have a functional NameGate instance that is ready for use. 

## ⚒️ Running as a Service
You can setup the NameGate executable to run as a service on both Linux and Windows

# 🐧 Linux (systemd)
Place it in `/etc/systemd/system/namegate.service`, start it with `systemctl start namegate`, enable it to start at boot using `systemctl enable namegate`.
Make sure to change the paths below to match where you extracted the files.

```
[Unit]
Description=NameGate service
After=network.target
StartLimitIntervalSec=0

[Service]
Type=simple
Restart=always
RestartSec=1
User=root
WorkingDirectory=/root/NameGate
ExecStart=/root/NameGate/NameGate --urls=http://0.0.0.0:80/

[Install]
WantedBy=multi-user.target
```

# 🪟 Windows
You create the service using the bit of powershell below. Make sure to change the path to match where you extracted the files.
```
$exePath = "C:\NameGate"
New-Service -Name "NameGate" -BinaryPathName "$exePath/NameGate.exe --contentRoot $exePath --urls=http://0.0.0.0:80/" -Description "NameGate service" -DisplayName "NameGate" -StartupType Automatic
Start-Service "NameGate"
```

To remove the service you use this:
```
Stop-Service "NameGate"
$service = Get-WmiObject -Class Win32_Service -Filter "Name='NameGate'"
$service.delete()
```

# 💥 Troubleshooting
## Service won't start
If the service won't start, make sure nothing else is listening on port 53.
You can locate the proces that's claiming the port using ` lsof -i :53` on Linux, and `netstat -abno` on Windows.

In my case (a Proxmox LXC container), `systemd-resolved` was claiming the port so I disabled it using `systemctl disable systemd-resolved`.

You can also look at the logs using the console if running standalone, or `journalctl -fu namegate -n 100` if running as a systemd service described above.

# 💡 Technologies
NameGate takes advantage of a number of cool technologies that I'm quite passionate about.
- Runs on .NET 8 and utilizes [Blazor](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor) Server to serve up the front end.
  - This simple UI is made up of [MudBlazor](https://mudblazor.com/) components.
- The DNS-side is handled by the [DNS library by kapetan](https://github.com/kapetan/dns) that I [forked](https://github.com/jrhodnik/dns) to expose additional data.
- [Entity Framework w/ Sqlite](https://learn.microsoft.com/en-us/ef/) is used for the various options and statistics.
- [FusionCache](https://github.com/ZiggyCreatures/FusionCache) is used for caching DNS query results, among other things.
- And many more I'm sure