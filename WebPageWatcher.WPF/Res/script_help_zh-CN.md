# 网页内容变动提醒 - 脚本语法说明

## 基本结构

脚本语句基本结构为“命令名 命令参数1 命令参数2 ...”。

没有代码块。

以空行为语句之间的分隔。

## 变量

变量类型标识|变量类型|实际类型
-|-|-
`string`|字符串|System.String
`response`|HTTP回应|System.Net.HttpWebResponse
`responseText`|字符串格式的HTTP回应结果|System.String
`responseJsonValue`|JSON格式HTTP回应的指定路径的值|System.String
`webpage`|网页对比对象|WebPageWatcher.Data.WebPage

## 命令

命令代码|命令名|基本语法
-|-|-
`let`|赋值|let 变量名 变量类型 用于新建该变量类型的对象的参数
`set`|设置属性|set WebPage变量名 WebPage对象 [clone]
`cp`|执行比较|cp WebPage变量名
`log`|日志输出|log 内容

### 赋值命令

赋值语句会因为变量类型的不同而不同。

变量类型|赋值参数1|赋值参数2|示例
-|-|-|-
`string`|字符串内容||`let data string id=43&token={token}`
`webpage`|webpage名|可选参数：`clone`|`let a3 webpage a3 clone`
`response`|`webpage`变量||`let a3r response a3`
`responseText`|`webpage`变量||`let a1r responseText a1`
`responseJsonValue`|`response`变量JSON结果的路径||`let a3Url responseJsonValue a2r.data.url`

#### `string`

字符串不加双引号`"`。

双引号若要内嵌其它string类型的变量值，需要在外侧加大括号`{}`

示例：

```
let string token abcdefg
let string para id=43&token={token}
```

#### `webpage`

`webpage`就是网页比较对象类型，与程序中的“网页”相对应。

第一个参数就是GUI中设置的名称。

若有第二个参数clone，则将新建一份该名称webpage的副本，防止对原对象产生了改变。

若有多个对象拥有相同的名称，那么将会报错。

#### `response`

该赋值命令执行时，将会尝试获取HTTP响应，并将对象暂时存在变量库中

#### `responseText`

该赋值命令执行时，将会尝试获取HTTP响应，并将对象转换成文字暂时存在变量库中

#### `responseJsonData`

该赋值命令执行时，将会尝试获取HTTP响应。将响应转换成文字后转换为JToken类型，然后根据参数中的路径，赋值指定的JToken。

例如，有一个`response`名为a，获得的JSON类型结果将为：

```
{
	"people": [{
			"firstName": "Brett",
			"lastName": "McLaughlin"
		},
		{
			"firstName": "Jason",
			"lastName": "Hunter"
		}
	]
}
```

需要获取第1个firstName，则代码为：
`let response a.people[0].firtName`

### 设置属性命令

使用`set`命令可以设置`webpage`对象的属性。

对于字符串、数字类型的属性，可以直接设置。例如：`set a3 Url a3Url`。

特别的，该命令还可以将某一个响应的所有Cookie应用到某一个`webpage`。

例如：有`webpage`类型变量`a4`，有`response`类型`a3r`。

则可以使用命令：`set a4 Cookies a3r`，将`a3r`的Cookie应用到`a4`。

### 执行比较命令

比较命令较为简单。由于该脚本没有返回值功能，因为比较后若有不同，将通过GUI进行返回。

### 输出日志命令

用于在调试窗口输出日志。