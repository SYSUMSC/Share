# MSC-Share

> 郑佳豪 ++

**编程世界概览**

---

# Overview

+ Programming Language
+ Algorithm
+ Linux
+ Version Control
+ Machine Learning
+ Embedded Development
+ Network
+ Web
+ Docker

---

# Programming Language

本部分参照 [编程语言概览 - huanghongxun](https://blog.csdn.net/huanghongxun/article/details/78279594)

+ 按运行类型分类
	+ 编译型、解释型 
+ 按编程方式分类
	+ 面向过程、面向对象、面向函数 
+ 按变量模式分类
	+ 静态、动态
+ 按内存管理分类
	+ 手动、自动 

---

# Programming Language

+ 编译原理
	+ [Babel](https://babeljs.io/) JavaScript Compiler
	+ 浏览器
	+ 富文本解析 WxParse
	+ C-To-MIPS 解放你的计组作业

---

# Algorithm

> 算法这一块，我贼菜

+ 一般算法类
	递推算法，各种排序算法（桶排序，基数排序，计数排序，快速排序，冒泡排序，选择排序，归并排序等等），贪心算法，递归算法，搜索算法（深搜，广搜），动态规划，分治算法，拓扑排序，FFT等等
  
+ 字符串类
	KMP，AC自动机等等

+ 数论类
	同余及应用，Eratosthenes/Euler法求素数，快速幂，扩展欧几里德算法，欧拉定理、CRT、Miller-Rubbin素性判断等等

---

# Algorithm

+ 图论类
	回路判断算法（哈密顿环等），强连通分量算法（kosaraju算法，tarjan算法等），最短路径算法（Floyd算法，Dijkstra算法，Bellman-Ford算法，SPFA算法等），最小生成树算法（kruskal算法，prim算法等），网络流相关（最大流，最小流等等）等等
+ 数据结构类
	堆，栈，队列，链表，哈希表，并查集，二分查找树，线段树，平衡树（红黑树，AVL，SBT），Splay树，Treap，树状数组，块状数组，仙人掌，带花树，主席树，跳舞链、Trie等等
+ 编译原理类
	NFA，DFA，CFG，正规表达式等等

---

# Linux

+ 基本命令
	+ `man` （爸爸级的指令
+ 链接库
	+ 动态链接库
	+ 静态链接库
+ 系统目录
	+ NB的技能：了解系统目录的结构及其对应功能
+ Vim
+ Shell脚本

---

# Version Control

+ 安利
	+ [GitHub](https://github.com) ~~大型同性交友平台~~
	+ [GitBook](https://www.gitbook.com/) 开源书籍的集散地

---

# Machine Learning

> 本人不会学习，所以也不会机器学习

+ 本质（你看“机器学习基石”就好了，逃
+ Resources
	+ 林轩田的[课程](https://www.csie.ntu.edu.tw/~htlin/mooc/)
		+ 机器学习基石
		+ 机器学习技法
	+ 莫烦的[个人网站](https://morvanzhou.github.io/tutorials/machine-learning/)
	+ 《机器学习》 周志华，俗称“西瓜书” 
+ Framework
	+ [Keras](https://keras.io/) （极力推荐，掉包侠必备
	+ [Tensorflow](https://www.tensorflow.org/)

---

# Machine Learning

+ 安利
	+ [kaggle](https://www.kaggle.com/) 听说你很喜欢刷题？
	+ [Google Colab](https://colab.research.google.com) 撸资本主义的羊毛
	+ [似然实验室](http://maxlikelihood.cn/) 每周日14点于南校新数学楼403开课
	+ 左大神介绍的两个项目
		+ 使用 [PyTorch](https://pytorch.org/) 重构一个使用 [Caffe](http://caffe.berkeleyvision.org/) 的项目
		+ 视频降噪与视频加帧

---

# Embedded Development

> 因为我很普通，下面列举的都是智能家居的应用 :)

+ [Arduino](https://www.arduino.cc/) 
	+ [Blink ](https://www.arduino.cc/en/Tutorial/Blink) Turn an LED on and off
+ [Raspberry Pi](https://www.raspberrypi.org/)
	+ A small and "affordable" computer
+ [Home-Assistant](https://github.com/home-assistant/home-assistant)
	+ A home automation platform running on Python 3 
+ Integration with AI
	+ Alexa
	+ Google Assistant

---

# Network

+ OSI
	+ 实体层、数据链路层、网络层、传输层、表达层、应用层
+ TCP 与 UDP
	+ TCP 保证数据传输的完整与有效，出现错误会重发
	+ UDP 不管数据传输的完整

---

# Web

> Web 的领域超出你的想象

+ [Web Developer-Roadmap](https://github.com/kamranahmedse/developer-roadmap)
+ 鉴权
+ 路由

---

# Web

+ 前端
	+ JavaScript HTML CSS
	+ 浏览器原理
	+ [jQuery](https://jquery.com/) [React](https://reactjs.org/) [Vue](https://vuejs.org/) [Angular](https://angular.io/)
	+ [Electron](https://electronjs.org/) 
		+ Build cross platform desktop apps with JavaScript, HTML, and CSS
	+ [React Native](https://facebook.github.io/react-native/)
		+ Build native mobile apps using JavaScript and React 

---

# Web

+ 后端
	+ Restful 与 GraphQL
	+ 语言与框架
	+ NoSQL 与 SQL
	+ 单体架构 与 微服务架构

---

# Docker

+ 不严格地说，你可把[Docker](https://www.docker.com/)理解成**超级轻量**的“虚拟机”
+ [Docker与虚拟机的区别](https://www.zhihu.com/question/48174633/answer/109868326)
+ 扩容变得如此简单
	```bash
    # 调整名称为 service 的服务的运行数目为 20
    docker-compose scale service=20
	```
+ [K8S](https://kubernetes.io/)

  + 由谷歌爸爸开发的一种强大的容器编排系统
+ 安利
	+ 微服务架构
    
---

# 谢谢大家

广撒网，多敛鱼，择“优”而从之