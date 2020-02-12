# 网页内容变动提醒

## 介绍

这是一款支持当指定的网页或接口响应发生变化时在桌面右下角弹出提醒框的小程序

## 截图

![image](/Screenshots/MainWindow.png)

## 特性

- 使用.Net Framework 4.8 + WPF 开发
- 使用MD风格的界面
- 支持GET和POST
- 支持HTML和JSON
- 支持中文和英文，支持扩展更多语言
- 能够跟随系统亮色/暗色模式进行自动切换


## 更新内容

- [x] 自动跟随系统主题变色
- [x] POST支持
- [x] JSON HTTP响应格式
- [x] 设置界面
- [x] 开机自启
- [x] 多次错误弹窗提示
- [x] 多国语言支持
- [x] 文本HTTP响应格式
- [x] 自动解析HTTP头
- [x] 铃声提醒
- [x] 强制检测
- [x] 二进制相应格式支持
- [x] 详细对比
- [x] 网站更新历史查看
- [x] 脚本语言
- [x] 界面逻辑分离
- [ ] 事件触发
- [ ] 全局开关


## 更新日志

### 2019-12-25

开始项目

基本完成主窗口搭建

基本完成Cookie设置窗口搭建

完成对Html的获取

### 2019-12-26

新增黑白名单窗口

新增HtmlElementIdentify类，用于定位元素

新增HtmlComparer类，编写了白名单比较方法，未测试

### 2019-12-27

继续完善黑白名单窗口

新增通过ID/XPath来定位HTML树状图的功能

将Dialog进行了提取封装

完成了简单的后台对比任务

### 2019-12-28
   
增加了黑白名单项的仅文字、忽略空白属性

基本完成了提醒窗口

新增更新时间间隔设置UI

首次成功实现了白名单对比autodotua.top成功

### 2019-12-29

完成了忽略HTML属性和忽略空白的选项，并实现了功能

新增了托盘图标，并支持根据系统的App颜色和系统颜色对托盘图标、主题进行亮色/暗色调整

修改了网页设置界面，改为三个Expander组成

增加了大多数的头文件设置

支持了POST，成功实现对我的成绩的获取

### 2019-12-30

对比界面新增了显示新旧源码

### 2019-12-31

增加支持了JSON格式的响应，暂时完成：

&nbsp;&nbsp;&nbsp;&nbsp;响应类型的HTML、JSON、TEXT的选择

&nbsp;&nbsp;&nbsp;&nbsp;JSON黑白名单的显示

&nbsp;&nbsp;&nbsp;&nbsp;相应后台逻辑的修改，如提取接口等

### 2020-01-05

完成了JSON对比核心代码

新增了显示上一次检查/更新时间以及上一次内容的窗口

完成了多国语言框架，完成了项目设置的翻译

修复了日期显示格式不符合本土化的BUG

### 2020-01-06

完成对多国语言化

页面将使用后台任务的数据源，保证二者统一

新增了当后台任务错误超过5次时，弹出错误框

### 2020-01-07

新增主界面菜单栏

新增设置窗体

新增语言设置项

新增开机自启设置项

修改数据目录为Windows指定目录而非程序目录

支持了网页变化时发出通知铃声提醒

### 2020-01-09

完成自动识别HTTP Header并且应用的功能

新增了完全退出程序的菜单项

新增支持强制进行一次对比以及进行一次获取的功能

### 2020-01-10

增加了二进制格式，并基本完成文本格式、二进制格式的获取、对比、查看等。为此，修改了基本数据格式为字节数组。

### 2020-01-11

基于Github的Google Diff算法，完成了显示前后网页详细区别的功能

### 2020-01-13

新增用于控制是否进行对比的开关

新增服务端选项，是否都发送Expect: continue-100

新增选项：Keep-Alive和是否允许跳转

基本完成了用于自动化处理的脚本语言的逻辑部分

### 2020-01-14

新增了脚本语法的查看入口和MarkDown显示功能（暂无内容）

新增主界面TabControl，用于切换WebPage页和之后将加入的脚本编辑、触发器页

使用了Material Design Extensions的Material Window窗体

新增了APP和窗体图标

### 2020-01-15

新增WebPage的Clone功能

基本完成Script界面，并把两个面板的功能做了提取

基本完成脚本的后台运行等逻辑

### 2020-01-16

分离了功能和界面，方便之后可能进行的跨平台。之前BackgroundTask中的与窗体交互的部分改为事件。

### 2020-01-17

新增了对解析错误的处理，当新内容或就内容单方面解析失败时，认为前后不一致（可选），双方都失败时，throw异常

新增了网页更新的记录以及历史记录的查看

优化了数据库结构，为了符合数据库设计范式，删除了WebPage的LatestContent列（原本是因为没有WebPageUpdate表所以暂时过渡）

### 2020-01-19

比较窗口新增差异对比

### 2020-01-23

新增后台任务总开关（菜单项）

新增查看所有历史记录（菜单项）

### 2020-01-24

新增支持导入导出

完成了触发器设置界面

### 2020-01-25

基本完成了触发器的逻辑

重新修改了异常处理洗头膏

新增日志系统并基本完成对获取、对比、脚本异常的采集以及后台任务的日志记录

### 2020-01-26

新增日志系统的显示

### 2020-02-12

分离了核心和界面的Config