namespace Estrella.InterLib.Networking
{
    public enum InterHeader : ushort
    {
        Ping = 0x0000,
        Pong = 0x0001,
        Ivs = 0x0002,
        ClientReady = 0x003,
        ChangeZone = 0x004,
        ClientDisconect = 0x005,

        UpdateParty = 0x098,
        RemovePartyMember = 0x097,
        AddPartyMember = 0x0096,
        NewPartyCreated = 0x0099, // WORLD -> ZONE | DATA: GROUP ID
        PartyBrokeUp = 0x009A, // WORLD -> ZONE | DATA: GROUP ID
        CharacterLevelUp = 0x00102,
        CharacterChangeMap = 0x00105, //World -> ZONE |
        UpdateMoney = 0x00103, //Zone -> World
        UpdateMoneyFromWorld = 0x00104, //World -> Zone
        Auth = 0x0010,
        BanAccount = 0x0095,
        Assign = 0x0100,
        Assigned = 0x0101,

        Clienttransfer = 0x1000,
        Clienttransferzone = 0x1001,

        Zoneopened = 0x2000,
        Zoneclosed = 0x2001,
        Zonelist = 0x2002,

        Worldmsg = 0x3000,
        GetBroadcastList = 0x3001,
        SendBroiadCastList = 0x3002,

        FunctionAnswer = 0x4000,
        FunctionCharIsOnline = 0x4001,
        SendAddRewardItem = 0x4002, //World -> Zone
        ReciveCoper = 0x4003, //Zone -> World

        //Guild Shit
        ZoneAcademyMemberJoined = 0x4101,
        ZoneAcademyMemberLeft = 0x4102,
        ZoneAcademyMemberOnline = 0x4103,
        ZoneAcademyMemberOffline = 0x4104,
        ZoneGuildMessageUpdate = 0x4105,
        ZoneAcademyBuffUpdate = 0x4106,
        ZoneGuildMemberLogout = 4107,
        ZoneZoneGuildMessageUpdate = 4108,
        ZoneGuildMemberAdd = 4109,
        ZoneGuildMemberRemove = 4110,
        ZoneGuildMemberRankUpdate = 4111,
        ZoneGuildMemberLogin = 4112,
        ZoneGuildCreated = 4113,
        ZoneCharacterSetBuff = 4114,
        ZoneCharacterRemoveBuff = 4115
    }
}