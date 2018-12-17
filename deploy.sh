cd ~/NetCore/Community-Discord-BOT/src/CommunityBot
dotnet publish CommunityBot -r linux-arm -c Release
ssh pi@192.168.0.101 "pkill CommunityBot"
scp -r ~/NetCore/Community-Discord-BOT/CommunityBot/bin/Release/netcoreapp2.1/linux-arm/publish/* pi@192.168.0.101:~/bots/miunie
ssh pi@192.168.0.101
