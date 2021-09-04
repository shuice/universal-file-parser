# universal-file-parser

[简体中文](https://github.com/shuice/universal-file-parser/blob/master/README-zh-cn.md)

[English](https://github.com/shuice/universal-file-parser/blob/master/README.md)

## Introduce
This software is an imitation, the source of imitation is "synalyze it", you can learn more from the website [https://www.synalysis.net/](https://www.synalysis.net/), if you want, you can Download from the above website and pay for a trial, but if you don’t have the ability to support it temporarily, you can use this software to try it out. This software is only available in the Windows UWP version at [https://www.microsoft.com/store/apps/9PH7MXQ5QFTZ]( https://www.microsoft.com/store/apps/9PH7MXQ5QFTZ), a free trial period of one month, no functional restrictions during the trial period, this software has no intention of forming a competitive relationship with "synalyze it".

## Why imitate
Because I particularly like to analyze the format of many files, I wanted to make a similar software around 2009, but after many twists and turns, I found that my ideas were limited and could not be realized. Until about 2014, I saw the software "synalyze it", and I paid for it and contributed some grammars and was adopted, so I have a deeper understanding of this software. The more I understand, the more I find that I can implement it. In order to satisfy my wish, I realized it.

## How to compile
1. First, you need to download the `Visual Studio 2019` and install the UWP development module in C# language. I have not tested the version above Visual Studio 2019, because the code is all C#, so the problem should not be big. If there is a problem, please let me know
2. Use vs2019 to open the `file_structure.sln` in the directory, compile and run the `Editor` project.


## Project Introduction
After opening the .sln file, you can see that sln contains more than ten projects, and the meaning of each project is as follows

#### 1. IronPython

A library for parsing python scripts, after modification, the UWP environment can be used. If it is a non-UWP environment, you can directly use the IronPython2 library in nuget.

    
#### 2. Editor

A UWP application, it does not contain a specific page, it just defines the application type, it is just a launcher, the actual page is defined in the project file_structure, and the Editor project references file_structure.

#### 3. file_structure
All UWP pages

#### 4. kernel
The library based on the analysis of grammar and file is the core of the entire program, including identifying and executing scripts. Most of the bugs should come from this.

#### 5. mp4
The same as Editor, it is a UWP application, this application can only analyze mp4 files, as a test product before the Editor function is completed, also it is just a launcher, start the project file file_structure.

#### 6.NeoLua
Lua script parser, used by the kernel, also be modified to adpte UWP environment.

#### 7.sample
A console application that show the results of test analysis, Samples and output it on the console to test whether the grammar is parsed normally. This application is also used to learn grammar, understand the types of node definitions in grammar, etc.

## Additional directory introduction

#### Build
Contains some files needed to compile IronPython2, please do not modify it, it comes from the official code of IronPython2, and generally does not need to be modified

#### gramma\Grammars
Grammar from synalyze it, the download address is [https://github.com/synalysis/Grammars/](https://github.com/synalysis/Grammars/), the license is MIT
#### gramma\Samples
Some sample files for testing grammar

## Other platforms
The software supports .net standard 2.0, so you can switch to other platforms for development. If you need to develop on other platforms, you need to compile the kernel. The kernel depends on IronPython and NeoLua.

## contribute
- You are welcome to submit a sample of documents with incorrect parsing, and you need to indicate what the correct situation should look like.

- Due to economic reasons, there is not much maintenance time, I'll try my best.


## License
-The project adopts dual licenses. If it is non-commercial, you can use and modify it at will, but please indicate the source. If it is commercial, please contact support@bami-tech.net.