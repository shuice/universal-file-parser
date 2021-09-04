# universal-file-parser

[简体中文](https://github.com/shuice/universal-file-parser/blob/master/README-zh-cn.md)

[English](https://github.com/shuice/universal-file-parser/blob/master/README.md)

## 介绍
这个软件是一个仿品，模仿来源是"synalyze it"，可以从网站[https://www.synalysis.net/](https://www.synalysis.net/)了解详情，如果你愿意，可以从上述网站下载并付费试用，但是如果你暂时没有能力支持，可以用本软件来试用，本软件只有Windows UWP版本，地址在[https://www.microsoft.com/store/apps/9PH7MXQ5QFTZ](https://www.microsoft.com/store/apps/9PH7MXQ5QFTZ)，免费试用一个月，试用期间无功能限制，本软件无意和"synalyze it"构成竞争关系。

## 为什么要仿造
因为我特别喜爱分析很多文件的格式，在2009年左右就想做一个类似的软件，但是几经周折，发现自己的思路有限，无法实现，直到大约2014年，看到该软件"synalyze it"，并付费使用，而且贡献了一些grammar并被采用，所以对这个软件有比较深刻的理解，越理解越发现我可以实现它，为了满足自己的心愿，于是实现了。

## 如何编译
1. 首先需要下载Visual Studio 2019版本，并安装C#语言的UWP开发模块，Visual Studio 2019以上的版本我没有测试过，因为代码都是C#的，所以问题应该不大，如果有问题，欢迎告知
2. 利用vs2019打开目录下的`file_structure.sln`，编译并运行`Editor`工程即可。


## 工程介绍
打开sln文件后，可以看到sln包含十来个project，每一个project的含义如下

#### 1. IronPython

用来解析python脚本的库，经过修改使得UWP环境可以用，如果是非UWP环境，可以直接用nuget中的IronPython2库

    
#### 2. Editor

一个UWP应用，它不包含具体的页面，只是定义了应用类型，仅仅是一个启动器，实际的页面定义在工程file_structure内，Editor项目引用了file_structure

#### 3. file_structure
所有UWP的页面

#### 4. kernel
根据grammar和file分析得出结果的库，是整个程序的核心，包括识别和执行脚本，大部分的bug都应该来源于此
#### 5. mp4
同Editor，是一个UWP应用，这个应用只能分析mp4文件，作为Editor功能完成之前的一个试验品，同样它只是一个启动器，启动项目文件file_structure
#### 6.NeoLua
lua脚本解析器，被kernel使用，同样也被修改来适应UWP环境

#### 7.sample
一个控制台应用，测试分析的结果，并在控制台输出，用来测试grammar是否解析正常，这个应用还用来学习grammar，理解grammar内的节点定义种类等等

## 额外的目录介绍

#### Build
包含一些编译IronPython2需要的文件，请不要修改，来源于IronPython2官方代码，一般不需要修改

#### gramma\Grammars
来自synalyze it的grammar，下载地址是[https://github.com/synalysis/Grammars/](https://github.com/synalysis/Grammars/)，许可证是MIT
#### gramma\Samples
一些测试grammar的样本文件

## 其他平台
软件支持.net standard 2.0, 所以可以切换到其他平台进行开发，如果需要在其他平台进行开发，需要把kernel编译通过，kernel依赖IronPython和NeoLua.

## 贡献
- 欢迎提交解析错误的文件样本，需要表明正确情况应该是什么样的
- 受制于经济原因，没有太多的维护时间，我尽量


## 许可证
- 该项目采取双许可证，如果是非商用，可以随意使用和修改，但请注明出处，如果是商用，请联系support@bami-tech.net。