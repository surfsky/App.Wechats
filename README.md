# 1 关于

- 微信公众号、小程序、支付相关操作类库。
- 作者：surfsky.github.com

# 2 安装
```

Nuget: install-package App.Wechats
```

# 3 类库结构

```
App.Wechats         : 本类库根命名空间
    Wechat          : 微信基类，包括一些公用的代码
    WechatConfig    : 微信配置信息
App.Wechats.MP      : 微信小程序命名空间
    WechatMP        : 微信小程序类
App.Wechats.OP      : 微信公众号命名空间
    WechatOP        : 微信公众号类
    PushMessage     : 微信公众号推送消息
App.Wechats.Pay     : 微信支付命名空间
    WechatPay       : 微信支付类

```

# 4 使用

## 4.1 配置参数

    （1）编写web.config中的appsetting部分。如：

    <!-- 微信公众号 -->
    <add key="WechatOPAppID" value="-" />
    <add key="WechatOPAppSecret" value="-" />
    <add key="WechatOPPayUrl" value="Pay.ashx" />
    <add key="WechatOPPushToken" value="-" />
    <add key="WechatOPPushKey" value="-" />
    <add key="WechatOPTokenServer" value="GetAccessToken?type=Open&amp;refresh={0}" />

    <!-- 微信小程序 -->
    <add key="WechatMPAppID" value="-" />
    <add key="WechatMPAppSecret" value="-" />
    <add key="WechatMPPayUrl" value="Pay.ashx" />
    <add key="WechatMPPushToken" value="-" />
    <add key="WechatMPPushKey" value="-" />
    <add key="WechatMPTokenServer" value="GetAccessToken?type=MP&amp;refresh={0}" />

    <!-- 微信商户 -->
    <add key="WechatMchId" value="-" />
    <add key="WechatMchKey" value="-" />

    (2) 或直接给 WechatConfig 类参数赋值，如

    WechatConfig.OpenAppId = "AppId";
    WechatConfig.OpenAppSecret = "AppSecret";

## 4.2 使用微信公众号接口

请使用 App.Wechats.Open.WechatOpen 类；
微信公众号推送消息处理请使用 App.Wechats.Open.PushMessage 类

## 4.3 使用微信小程序接口

请使用 App.Wechats.MP.WechatMP 类

## 4.4 使用微信支付接口

请使用 App.Wechats.Pay.WechatPay 类

## 4.4 日志

```c#
// 微信接口操作日志
WechatConfig.Instance.OnLog += (name, user, request, reply) =>
{
    var msg = new { Request = request, Reply = reply }.ToJson();
    Logger.LogDb(name, msg, user);
};
```

# 5 History

1.1
    
    发布1.1版本，包含依赖 App.Core
    2019-10

1.2
     
    去除 App.Core 依赖
    2020-09
