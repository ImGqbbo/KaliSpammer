using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaliSpammer.Discord
{
    public enum DiscordSpeakingFlags
    {
        Microphone = 1 << 0,
        Soundshare = 1 << 1,
        Priority = 1 << 2
    }
}
