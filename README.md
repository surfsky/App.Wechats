# About

微信公众号、小程序、支付相关操作类库。
surfsky.github.com

# Install
```

Nuget: install-package App.Corer
```

# 类库结构

```
App.Wechats         : 本类库根命名空间
    Wechat          : 微信基类，包括一些公用的代码
    WechatConfig    : 微信配置信息
App.Wechats.MP      : 微信小程序命名空间
    WechatMP        : 微信小程序类
App.Wechats.Open    : 微信公众号命名空间
    WechatOpen      : 微信公众号类
    PushMessage     : 微信公众号推送消息
App.Wechats.Pay     : 微信支付命名空间
    WechatPay       : 微信支付类

ps.本项目依赖 App.Core.dll，请自行引用
```

# 使用

##  配置参数

    （1）编写web.config中的appsetting部分。如：

    <!-- 微信公众号 -->
    <add key="WechatOpenAppID" value="-" />
    <add key="WechatOpenAppSecret" value="-" />
    <add key="WechatOpenPayUrl" value="Pay.ashx" />
    <add key="WechatOpenPushToken" value="-" />
    <add key="WechatOpenPushKey" value="-" />
    <add key="WechatOpenTokenServer" value="GetAccessToken?type=Open&amp;refresh={0}" />

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

## 使用微信公众号接口

请使用 App.Wechats.Open.WechatOpen 类；
微信公众号推送消息处理请使用 App.Wechats.Open.PushMessage 类

## 使用微信小程序接口

请使用 App.Wechats.MP.WechatMP 类

## 使用微信支付接口

请使用 App.Wechats.Pay.WechatPay 类


# 计划

- 插入日志接口或事件（记录请求数据、返回数据、解析数据），便于调试记录
- 编撰使用文档

