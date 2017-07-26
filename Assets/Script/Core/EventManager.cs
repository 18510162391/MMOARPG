using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class EventManager<T>
{
    //private static EventManager<T> _instance;
    //public static EventManager<T> Instance
    //{
    //    get
    //    {
    //        if (_instance == null)
    //        {
    //            _instance = new EventManager();
    //        }
    //        return _instance;
    //    }
    //}

    private static Dictionary<EvtType, List<Action<T>>> m_EventListeners;

    public static Dictionary<EvtType, List<Action<T>>> EventListeners
    {
        get
        {
            if (m_EventListeners == null)
            {
                m_EventListeners = new Dictionary<EvtType, List<Action<T>>>();
            }
            return m_EventListeners;
        }
    }

    /// <summary>
    /// 添加事件监听者
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="onComplete">事件回调</param>
    public static void AddEventListener(EvtType eventType, Action<T> onComplete)
    {
        if (EventListeners.ContainsKey(eventType))
        {
            var eventListeners = EventListeners[eventType];
            if (!eventListeners.Exists(p => p == onComplete))
            {
                eventListeners.Add(onComplete);
            }
        }
        else
        {
            List<Action<T>> eventlisteners = new List<Action<T>>();
            eventlisteners.Add(onComplete);

            EventListeners.Add(eventType, eventlisteners);
        }
    }

    /// <summary>
    /// 移除事件监听
    /// </summary>
    /// <param name="eventType"></param>
    public static void DropAllEventListener(EvtType eventType)
    {
        if (EventListeners.ContainsKey(eventType))
        {
            EventListeners.Remove(eventType);
        }
    }


    /// <summary>
    /// 移除事件监听
    /// </summary>
    /// <param name="eventType">类型</param>
    /// <param name="onComplete">回调函数</param>
    public static void DropEventListener(EvtType eventType, Action<T> onComplete)
    {
        if (EventListeners.ContainsKey(eventType))
        {
            if (EventListeners[eventType].Contains(onComplete))
            {
                EventListeners[eventType].Remove(onComplete);
            }
        }
    }

    /// <summary>
    /// 派发消息
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="arg"></param>
    public static void DispathEvent(EvtType eventType, T arg)
    {
        if (EventListeners.ContainsKey(eventType))
        {
            var eventListeners = EventListeners[eventType];

            for (int i = 0; i < eventListeners.Count; i++)
            {
                try
                {
                    var callBack = eventListeners[i];
                    callBack(arg);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                    continue;
                }

            }
        }
    }
}

/// <summary>
/// 事件消息类型
/// </summary>
public enum EvtType
{
    None = 0,

    /// <summary>
    /// 角色红点显示
    /// </summary>
    ShowPlayerRed = 1,

    /// <summary>
    /// 显示称号红点
    /// </summary>
    ShowTitleRed = 3,


    ERidesMainInterfaceRed = 4,           // 坐骑主界面图标红点显示

    WingRed = 10,//翅膀界面红点
    AmuletRed = 11,//符咒界面红点
    RuneRed = 12, //符文界面红点

    /// <summary>
    /// 穿戴装备
    /// </summary>
    WearEquip = 13,

    /// <summary>
    /// 按键启用
    /// </summary>
    KeyCodeEnabled = 14,

    /// <summary>
    /// 点击帮派科技洗练按钮
    /// </summary>
    ReceiveXiLian = 15,

    /// <summary>
    /// 收到洗练保存
    /// </summary>
    ReveiveXiLianSave = 16,

    /// <summary>
    /// 收到帮派科技升级成功
    /// </summary>
    ReceiveGangTechUpgrade = 17,


    ArtifactRed = 18,  //神器界面红点显示
    CommandExecuted = 19,
    /// <summary>
    /// 开始欢迎界面倒计时
    /// </summary>
    BeginSecondUpdate,

    MopUpMaterial,
    //排行榜tips装备外显
    EquipOuter = 21,

    ReceiveMaterialCopyInfo = 22,
    ReceiveMaterialCopyReset = 23,

    ReceiveHangMachine = 24,
    GangTaskFinsh = 25,
    GangActivityInit = 26,
    GangActivityApplySucceed = 27,
    GangActivityJoinSucceed = 28,
    GangBossInitComplete = 29,
    GangBossHurtRewardGet = 30,
    /// <summary>
    /// 帮派boss副本 导航栏初始化
    /// </summary>
    GangBossCopyInit = 31,

    // 山海界养成特效提示
    ShanHaiIsShowTips = 32,

    GangBossHPUpdate = 33,

    /// <summary>
    /// 是否显示微端特效提示
    /// </summary>
    WeiDuanShowTips = 34,

    /// <summary>
    /// 是否显示福利大厅按钮特效
    /// </summary>
    IsShowSignEffect = 35,


    /// <summary>
    /// 是否显示福利大厅按钮特效
    /// </summary>
    BackPackOpen = 36,

    /// <summary>
    /// 仓库解锁
    /// </summary>
    ItemStorageOpen = 37,

    /// <summary>
    /// 是否显示成就按钮特效
    /// </summary>
    IsShowAchieveEffect,
    AllStageFuncOnCoinCopy,

    UpdateMainRedOfEquip,

    /// <summary>
    /// 主界面世界BOSS按钮红点事件
    /// </summary>
    WorldBossRed,

    /// <summary>
    /// Boss大厅主界面按钮红点事件
    /// </summary>
    BossHallRed,
    IngotGift,
    //神将
    GodOpenInit,
    GodUpStar,
    GodIsClose,
    GodIconRed,

    //神兽
    GodAniOpenInit,
    GodAniUpStar,
    GodAniIsClose,
    GodAniIconRed,

    //新坐骑
    SkyOpenInit,
    SkyUpStar,
    SkyIsClose,
    SkyIconRed,

    //神灵
    GodSpiritOpenInit,
    GodSpiritUpStar,
    GodSpiritIsClose,
    GodSpiritIconRed,
    GodIsShow,
    GodSpiritChuZhanSucceed,
    GodSpiritActiveSucceed,//神灵激活成功
    GodSpiritUseQianNengSucceed,

    //绑定手机
    BindingPhone,
    BindingRed,

    //精彩活动
    WonderfulRed,
    WonderfulShow,


    //大厅活动
    HallprivilEgesRed,
    HallprivilEgesShow,

    //特权
    QQHallprivilEgesRed,
    QQHallprivilEgesShow,

    //YY大厅
    YYHallprivilEgesRed,
    YYHallprivilEgesShow,

    //蓝钻续费
    ReneWalRed,
    ReneWalSsow,

    //每日充值
    EveryAccShow,
    EveryAccRed,
    EveryLoginEffect,

    //360快捷支付
    Rechange360IsShow,

    #region 副本红点
    CopynRed,
    #endregion

    #region 山海之路红点
    KingCopynRed,

    #endregion
    #region 倒卖
    /// <summary>
    /// 倒卖状态，银票信息,倒计时,购买，出售，城市，排行，兑换，打开面板，刷新（不刷新）
    /// </summary>
    MyResellingState,
    MyResellingItem,
    MyDownCount,
    MyRanakOne,
    ExchangeBuyItem,
    GetCurCount,
    OpenNpcUpdateUI,
    beginningPrice,
    ItemsPrice,
    ChangeItemMoney,
    BuyAndSell,
    RecordInfo,
    NoRefresh,
    RankInfoList,
    MyExChange,
    ExChangeItem,
    UpdatePrices,
    ReseIconRed,
    #endregion

    #region 单人竞技场红点
    ArenaIconRed,
    ArenaJJ_pointChange,
    #endregion

    #region 技能红点
    SkillRed,
    #endregion

    #region
    MopUpIsClose,
    #endregion

    #region  猎魔
    RazorIconRed,
    #endregion

    #region 活动大厅
    ActiIconRed,
    #endregion

    #region 添加Buff描述
    BufferText,
    #endregion

    #region 副本相关
    /// <summary>
    /// 通用副本剩余次数初始化
    /// </summary>
    InitCopySurplusJoinNumComplete,
    OnPickUpRewardSucceed,
    MainPlayerRelive,
    CopyMopUpCondition,
    CopyMopUpSucceedd,
    CopyMopUpCD,

    #region 大乱斗
    OnDaLuanDouMemberInfoChange,
    UpdateRewardBox,
    #endregion

    #region 通天塔
    ClimbRed,
    #endregion
    #region 帮派战
    GangBattleBeginCountDown,
    GangBattleCountDownEnd,
    GangBattleTotemBelongInit,
    GangBattleTotemBelongChanged,
    GangBattleTotemBelongUpdateUI,
    GangBattleKillRankChanged,
    GangBattleKillRankUpdateUI,
    GangBattleKillPlayerInfo,
    GangBattleKillPlayerUpdateUI,
    GangBattleReward,
    GangBattleOnReceiveReward,
    GangBattleContriuteKillInit,
    AddHealthComplete,
    #endregion

    #region PVP
    /// <summary>
    /// 初始化pvp界面中匹配按钮状态
    /// </summary>
    updateActivityState,
    updateBeginMatchCountDown,
    OnPVPMemberInfoChange,
    UpdateMatchingIcon,
    UpdateCrossServerMainWindowEffect,
    #endregion
    #region  福利大厅
    SignWindowInit,
    SignSucceed,
    GetTotalSignReward,
    GetLevelReward,//获得等级奖励
    OnlineRewardWindowInit,// 在线奖励面板初始化
    UpdateOnlineRewardTime,
    UpdateOnlineCountDown,
    OnGetOnlineRewardSucceed,
    OnGetOffLineReward,
    OnCopyFindRewardSucceed,
    updateSignEffect,
    updateLevelRewardEffect,
    updateOnlineEffect,
    updateOfflineEffect,
    updateCopyFindEffect,
    updateWelfareEffect,//福利大厅主界面特效
    OnLineRewardAllGot,
    #endregion

    #region 登录奖励
    OnGetLoginReward,
    UpdateLoginCountDown,
    OnLoginCountDownEnd,
    UpdateLoginRewardIcon,
    UpdateLoginIconContent,
    SetLoginRewardIconActive,
    OnLoginRewardInit,
    #endregion
    /// <summary>
    /// 副本掉落经验
    /// </summary>
    GetExp,

    /// <summary>
    /// 副本掉落金币
    /// </summary>
    GetCoin,

    #endregion
    /// <summary>
    /// 玩家等级提升
    /// </summary>
    PlayerUpGrad,

    ZhizunTeQuan,
    JuanXianItemChanged,
    GangMemberJobChanged,

    EquipRed,    //装备强化红点

    SeaWill,        //山海志刷新

    LianShenShengJi,//炼神可升级

    GodWeaponRed,  //神兵主界面按钮红点

    EquipSuitRed,      //装备套装红点

    OpenServerRed,          //开服活动红点

    AstrologyRed,     //新版星宿主界面按钮红点

    /// <summary>
    /// 山海印（旧版：文位）主界面按钮红点
    /// </summary>
    CivilianRed,

    CivilianLevelChange,  //山海印等级变化

    LevelingRed,            //练级谷特效

    UpLevelDayRed,//升阶特惠红点
    UpLevelDayIconChange,//升阶特惠红点

    GangStorageChanged,
    HighInvestRed,//高倍投资红点

    SecretStoreRed,//神秘商店
    FirstRechargeRed,//首次充值
    FirstRechargeLoginEffect,
    PKValueChanged,//pk值变化

    DialLottery,  //转盘活动开启状态监测
    DialRechargeRed, //幸运转盘红点

    ComposeItem,        //合成返回
    ComposeItemRed,     //合成红点

    VIPLevelChange,     //VIp相关变化
    VIPRewardChange,    //VIP礼包领取变化
    MingUpgrade,//命格升级发生变化
    MingGeDataChanged,//命格数据发生改变

    RefreshBossPrompt, //boss刷新提示倒计时
    SweepOverRefresh,  //个人boss扫荡，挑战次数重置

    NewTitleRed,        //新称号
    AgainRechargeRed,     //续充
    GoldDiamondRed,      //金钻特权
    AddictionIsShow,     //刷新防沉迷按钮显示
    AddictionTipRefresh,   //防沉迷tips刷新


    #region 资源监控
    AssetInstantiate,
    AssetDestroy,
    #endregion


    //10000以下都为通用事件，不要写下面！！！
    ILevelUpEvt = 10000, //主角升级
    IGetCoinEvt = 10001, //货币变化
    IGetItemEvt = 10002, //道具变化
    IEquipChangeEvt = 10003,//主玩家穿戴装备发生变化
    IMianPlayerPropertyChange = 10004,//主玩家属性发生变化
    IActivityStateChange = 10005,//活动状态发生改变（没开始、预告、已经开始、已经结束）
    IActivityCountDown = 10006,//活动倒计时uplevel
    ICDStateChange = 10007,  //cd状态发生变化
    ICDCountDown = 10008,   //cd倒计时
    IOpenFunction = 10009,  //开启功能
    IFightValueChange = 10010,  //战斗力改变
    IGetCoinTypeEvt = 10011,//货币变化 参数是货币类型
    IOtherPlayerLevelUp = 10012,//其他玩家升级
}

public class EventRideRedArg
{
    public bool IsShowRed = false;

    public int suipian = 0;
}

/// <summary>
/// 装备外显
/// </summary>
public class Equip_Outer
{
    public int headid = 0;
    public int weaponid = 0;
    public int dressid = 0;
}
