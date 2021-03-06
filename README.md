# OneData [![Tweet](https://img.shields.io/twitter/url/http/shields.io.svg?style=social)](https://twitter.com/intent/tweet?text=Get%20the%20easiest,%20feature-rich%20ORM%20you%20were%20looking%20for.&url=https://github.com/jorgemk85/OneData&via=GitHub&hashtags=orm,design,c%23,database,dataaccess,developers)
[![OneData](https://img.icons8.com/cotton/64/000000/cloud-database.png)](https://github.com/jorgemk85/OneData) 

[![Build Status](https://img.shields.io/badge/build-passing-green.svg)](https://github.com/jorgemk85/OneData) [![Build Stage](https://img.shields.io/badge/stage-alpha-yellowgreen.svg)](https://github.com/jorgemk85/OneData) [![CodeFactor](https://www.codefactor.io/repository/github/jorgemk85/onedata/badge)](https://www.codefactor.io/repository/github/jorgemk85/onedata)


So, you are looking for a VERY easy, Code-First solution to access your data inside a MySQL or MsSQL server...

## Features
* Very fast!
* Reactive and Preventive modes.
* IL to Get and Set values within properties (~300% increased performance against Reflection).
* Ease of relational data access.
* Automatic class/model binding.
* In-RAM per class/model cache.
* Automatic class/model structure sync inside your database.
* Automatic logging in disk and database of every transaction (optional)
* Supports simultaneous database connections.
* Use your default database connection or switch with ease to use a diferent one.
* Very easy query pagination on database.
* Strongly typed.
* Supports Async.
* Events for every transaction type (Insert, Select, Update, Delete, etc).
* Per class/model configuration.
* Get foreign data into your desired "local" property with ease.
* Massive operations are supported! You can insert, update or delete in your database with ease.
* Call any stored procedure of your liking with ease.

... And much more!

Don't know SQL? Why should you? OneData got you covered ;)

## Compatibility
OneData is developed using C# and is compatible with any project that use the following:
* .Net Framework 4.6.1 and later.
* .Net Standard 2.0 and later.
* .Net Core 2.0 and later.

>Are you using VB.net to develop your apps? Dont' worry, you can still use OneData without any kind of problems.

When talking about database engines, OneData is ment to work with:
* MySQL
* Microsoft SQL

## Steps
Now that we got all the intro out of our way, let's stablish what tasks must be done to get your project up and running with OneData:

1) Install the package.
2) Configure your project .config file.
3) Setup your classes.

DONE!

## Installation

Download the library from [GitHub](https://github.com/jorgemk85/OneData/). 

OR

Install it via [NuGet](https://www.nuget.org/packages/OneData/).

#### Package Manager
```
Install-Package OneData
```

#### .NET CLI
```
dotnet add package OneData
```

*Can't find it with NuGet Package Manager? Make sure you enable "Include prerelease" checkbox inside your NuGet Package Explorer.*

## Configuration

Now that you have OneData installed in your project, you need to set up your .config configuration file (if you are using .Net Framework) or your .json configuration file (if you are using .Net Core or .Net Standard). [Should I use .Net Standard?](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)

### .Net Framework 4.6.1 and Later
In case you don't have the connectionStrings section inside your configuration file, please add it (don't include the placeholders <>):
```xml
  <connectionStrings>
    <add name="<your connection name>" connectionString="server=<your server ip / hostname>;Uid=<db username>;Pwd=<db password>;persistsecurityinfo=True;database=<your database name>;SslMode=none;AllowUserVariables=True;CheckParameters=False" />
  </connectionStrings>
```
Notice that there are some special settings in the connection string, only applicable if you are using MySQL. Make sure you include them.
```
SslMode=none;AllowUserVariables=True;CheckParameters=False
```
The following is a complete list of the configurations available (don't include the placeholders <>).

Please add them ALL to your project:
```xml
  <appSettings>
    <add key="DefaultConnection" value="<your default connection name>" />
    <add key="ConnectionType" value="<MySQL or MSSQL>" />
    <add key="InsertSuffix" value="<the suffix to use in the INSERT SPs>" />
    <add key="UpdateSuffix" value="<the suffix to use in the UPDATE SPs>" />
    <add key="DeleteSuffix" value="<the suffix to use in the DELETE SPs>" />
    <add key="StoredProcedurePrefix" value="<the prefix for all the SPs>" />
    <add key="TablePrefix" value="<the prefix for all the tables>" />
    <add key="EnableLogInDatabase" value="<true or false>" />
    <add key="EnableLogInFile" value="<true or false>" />
    <add key="DefaultSchema" value="<name of your default schema>" />
    <add key="IsReactiveModeEnabled" value="<true or false>" />
    <add key="IsPreventiveModeEnabled" value="<true or false>" />
  </appSettings>
```
### .Net Standard 2.0 and Later / .NET Core 2.0 and Later
In case you don't have the ConnectionStrings section inside your configuration file, please add it (don't include the placeholders <>):
```json
  "ConnectionStrings": {
      "<your connection name>": "server=<your server ip / hostname>;Uid=<db username>;Pwd=<db password>$;persistsecurityinfo=True;database=<your database name>;SslMode=none;AllowUserVariables=True;CheckParameters=False"
    }
```
Notice that there are some special settings in the connection string, only applicable if you are using MySQL. Make sure you include them.
```
SslMode=none;AllowUserVariables=True;CheckParameters=False
```
The following is a complete list of the configurations available (don't include the placeholders <>).

Please add them ALL to your project:
```json
  "AppSettings": {
    "DefaultConnection": "<your default connection name as stated in ConnectionString>",
    "ConnectionType": "<MySQL or MSSQL>",
    "InsertSuffix": "<the suffix to use in the INSERT SPs>",
    "UpdateSuffix": "<the suffix to use in the UPDATE SPs>",
    "DeleteSuffix": "<the suffix to use in the DELETE SPs>",
    "StoredProcedurePrefix": "<the prefix for all the SPs>",
    "TablePrefix": "<the prefix for all the tables>",
    "EnableLogInDatabase": "<true or false>",
    "EnableLogInFile": "<true or false>",
    "DefaultSchema": "<name of your default schema>",
    "IsReactiveModeEnabled": "<true or false>",
    "IsPreventiveModeEnabled": "<true or false>"
  }
```

Ok, that wasn't hard, isn't it? We are done with the configuration!
## Setup

The last step is to setup your classes!

But... Which one's should I setup? Well, every class you will need to connect / have-access-to in your database.

Here's an example with the minimum required setup for the library to understand your class:
```c#
using OneData.Attributes;
using OneData.DAO;
using OneData.Interfaces;

[DataTable("logs")]
public class Log : IManageable
{
    [PrimaryKey]
    public Guid Id { get; set; }
    [DateCreated]
    public DateTime DateCreated { get; set; }
    [DateModified]
    public DateTime DateModified { get; set; }

    public ModelComposition GetComposition()
    {
        return Manager<Log>.Composition;
    }
}
```

The attributes `DataTable`, `PrimaryKey`, `DateCreated` and `DateModified` will be explained with detail later in this document. For now, know that they are used to indicate which property is used for what specific purpose. The name of your properties is completely up to you.

Please note the IManageable implemented method `GetComposition` which NEEDS to return Manager<`YOUR CLASS TYPE`>.Composition. As the example shows, `Log` is the class name and return for GetComposition is `Manager<Log>.Composition`. This is very important to setup properly since the compiler MIGHT not show a compilation error (it varies from class to class) but would fail during runtime.
  
Well done! You now have an up and running a complete relational data management solution inside your project.
## Usage
### Configuration:
We have already explained where to put this configurations but haven't detailed what they are.

The following table is a comprehensive list of available configurations with their respective information:

| Configuration name          | Remarks									| Description                    |
|-----------------------------|-----------------------------------------|--------------------------------|
|`DefaultConnection`          |None.                                    |Type the name of your default connection.|
|`ConnectionType`             |MySQL or MsSQL                           |Choose to configure OneData for MySQL or MsSQL.|
|`InsertSuffix`               |Can be Blank.                            |Literally the suffix to use with the Insert SPs.|
|`UpdateSuffix`               |Can be Blank.                            |Literally the suffix to use with the Update SPs.|
|`DeleteSuffix`               |Can be Blank.                            |Literally the suffix to use with the Delete SPs.|
|`StoredProcedurePrefix`      |Can be Blank.                            |Prefix for every Stored Procedure.|
|`TablePrefix`				  |Can be Blank.                            |Prefix for every Table.|
|`EnableLogInDatabase`        |true or false                            |Choose to enable logging inside the database.|
|`EnableLogInFile`			  |true or false                            |Choose to enable logging on disk.|
|`DefaultSchema`              |For MySQL it's the database name.        |Type the name of your default schema/database.|
|`IsReactiveModeEnabled`      |true or false.							|OneData will react according to the problem that is detected and will fix it. No preventive actions are taken.|
|`IsPreventiveModeEnabled`    |true or false.							|Heuristic approach to sync everything in your database based on your classes/models, preventing problems before they appear.|

### Attributes:
Attributes in OneData are used to configure the classes/models and properties.

The following table is a comprehensive list of available attributes with their respectic information:

| Attribute name        | Used with  | Remarks                            | Description                    |
|-----------------------|------------|------------------------------------|--------------------------------|
| `AutoProperty`        | Properties | None.                              | Data is completely managed by OneData based on your settings.   |
| `CacheEnabled`        |  Classes   | Once per Class/Model.              | Enables a class/model to use the In-RAM Cache. Uses minutes as expiration.|
| `DataLength`          | Properties | None.                              | Specify which data length you want to use. If not implemented, will use default.|
| `DataTable`           |  Classes   | Required. Once per Class/Model.    | Sets the table name (and optinally the scheme) to use.|
| `DateCreated`			| Properties | Required. Once per Class/Model.    | Mark the property that will hold date and time of record creation.|
| `DateModified`		| Properties | Required. Once per Class/Model.    | Mark the property that will hold date and time of record update.   |
| `ForeignData`         | Properties | None.                              | Used when you need to get information from a foreign table.   |
| `ForeignKey`          | Properties | None.                              | Relates a property with the PrimaryKey of another class/model.   |
| `HeaderName`          | Properties | None.                              | Specify the name to look for instead of the propery name.   |
| `PrimaryKey`			| Properties | Required. Once per Class/Model.    | Mark the property that will be set as the PrimaryKey. |
| `Unique`				| Properties | None.                              | Set a property to hold a unique value.   |
| `Default`				| Properties | None.                              | Sets a default value to a property inside the database.   |
| `UnmanagedProperty`   | Properties | None.                              | Used when you don't want OneData to interfere with.   |

### Transactions
Let's talk a bit about transactions... First of all, we've got an enumeration called `TransactionTypes` which holds the following:
* Select
* SelectAll
* Delete
* DeleteMassive
* Insert
* InsertMassive
* Update
* UpdateMassive
* StoredProcedure

They are pretty self explanatory, except maybe `DeleteMassive`, `InsertMassive`, `UpdateMassive` and `StoredProcedure`. The first three types are used to execute the desired transaction but with a big set of data. The last one is used when you want to execute a generic stored procedure inside your database.

## Examples

### Basics
#### Changing my class/model
In section **Setup**, we stablished a class/model called `Log`, which only have three properties. If we run our program, OneData will be creating the respective table called "logs" (as configured with the `DataTable` attribute). This table will also have three columns which reflects our class/model... But, what if I add a new property? Should I go into the table manually and change it as I please? NO! You just need to add this new property to your class/model and let OneData take care of the rest (make sure you have the corresponding settings inside your .config file)! Please see the following:

This is our new `Log` class/model (note the new properties `UserId` and Transaction).
```c#
[DataTable("logs")]
public class Log : IManageable
{
    [PrimaryKey]
    public Guid Id { get; set; }
    [DateCreated]
    public DateTime DateCreated { get; set; }
    [DateModified]
    public DateTime DateModified { get; set; }

    public Guid UserId { get; set; }
    public string Transaction { get; set; }

    public ModelComposition GetComposition()
    {
        return Manager<Log>.Composition;
    }
}
```
Literally, the next time your program runs and tries to access this object in the database, OneData will make the changes it detected inside your class/model without prompting anything and as transparent as it should be.

This exact steps will trigger if your change is as small as adding a new property or huge as adding twenty, changing datatypes of another three, modifying the datalength of a couple and updating the relationships between classes/models.

> #### Warning
>
> Please note, OneData does NOT change column names. Once a property is created, it's name is used for the column inside your specified table. If you change your property name within your class, OneData will consider this as a drop column and proceed to add the "new" one. This might lead to **data loss**.
> If you need to rename your column, please do so manually in your database and make sure you reflect the changes in your class. This way, OneData will only adjust the related stored procedures without making any structural changes inside the table.

#### Adding data to the database
Let's say you have a single `Log` object with it's respective data already filled. How do I insert it into the corresponding table?
```c#
myNewLog.Insert();
```
A neat trick to ease your way when adding new objects of your desired type, is to add constructors to your class/model. This way, whenever you call `new` on your type, the Id will be filled automatically.

This pair is pretty handy:
```c#
public Log()
{
    Id = Guid.NewGuid();
}

public Log(Guid id)
{
    Id = id;
}
```

When you wish to insert a set of information contained in a `IEnumerable<T>`, say, a `List<T>`, you can simply do the following:
```c#
myLogCollection.InsertMassive();
```

OneData will then serialize your list and send it to the database for procesing, making just ONE call to insert every single one of your objects in the corresponding table. *Beware of your collection size, since even tho OneData has no cap or limit, your database or server might.*

#### Updating data in the database
Similarly to Insert, if you need to update a record, you can do the following:
```c#
myUpdatedLog.Update();
```
OneData uses the value inside the property identified as `PrimaryKey` to find the object in the database and update it as you wish. 

**The Update stored procedure uses IFNULL(), so if you want to send partial information, you should send your object with every property set to null except those you really need to update, and of course your `PrimaryKey` value should be set.**

In case that you need to set a value to `null` during an update, please use `QueryOptions` and set the property `UpdateNulls` to `true` or in the case that you are using the extension like in this example, set the parameter `updateNulls` to `true`;

```c#
myUpdatedLog.Update(true);
```

When you wish to update a set of information contained in a `IEnumerable<T>`, say, a `List<T>`, you can simply do the following:
```c#
myLogCollection.UpdateMassive();
```

OneData will then serialize your list and send it to the database for procesing, making just ONE call to update every single one of your objects in the corresponding table. *Beware of your collection size, since even tho OneData has no cap or limit, your database or server might.*

#### Deleting data in the database
Similarly to Insert or Update, if you need to Delete a record, you can do the following:
```c#
myUpdatedLog.Delete();
```
You only need to send the id of your record inside the property you identified as `PrimaryKey` to find the object in the database and delete it.

When you wish to delete a set of information contained in a `IEnumerable<T>`, say, a `List<T>`, you can simply do the following:
```c#
myLogCollection.DeleteMassive();
```

OneData will then serialize your list and send it to the database for procesing, making just ONE call to delete every single one of your objects in the corresponding table. *Beware of your collection size, since even tho OneData has no cap or limit, your database or server might.*

#### Selecting data from the database
Selecting data from the database is NOT performed using stored procedures as with Inserting, Updating or Deleting. This is because of the complex nature and wide variety of queries.

OneData uses lambda expressions to work with queries, making it very readable and of course, *refactor friendly* along the way.

You can use the provided `Manager<T>.Select` methods explained below to perform your queries, but OneData includes a helper class called `Extend<T>` that would result in extending your `IManageable` classes to ease your query code. You just need to inherit from it as shown in the following class:

```c#
using OneData.Attributes;
using OneData.DAO;
using OneData.Interfaces;

[DataTable("users")]
public class User : Extend<User>, IManageable
{
    [PrimaryKey]
    public Guid Id { get; set; }
    [DateCreated]
    public DateTime DateCreated { get; set; }
    [DateModified]
    public DateTime DateModified { get; set; }

    public string Name { get; set; }
    public string Lastname { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    public ModelComposition GetComposition()
    {
        return Manager<User>.Composition;
    }
}
```
Now you will be able to perform the following queries using the Extensions:

Returns a list of logs found in the database that match the provided `userId`.
```c#
private List<Log> GimmeAllTheLogsFromUserId(Guid userId)
{
	return Log.SelectList(q => q.UserId == userId);
}
```
Returns a list of logs found in the database that contain the provided `transaction`.
```c#
private List<Log> GimmeAllTheLogsThatContainTransaction(string transaction)
{
	return Log.SelectList(q => q.Transaction.Contains(transaction));
}
```
Returns a list of logs found in the database that starts with the provided `transaction`.
```c#
private List<Log> GimmeAllTheLogsThatStartsWithTransaction(string transaction)
{
	return Log.SelectList(q => q.Transaction.StartsWith(transaction));
}
```
Returns a list of logs found in the database that ends with the provided `transaction`.
```c#
private List<Log> GimmeAllTheLogsThatEndsWithTransaction(string transaction)
{
	return Log.SelectList(q => q.Transaction.EndsWith(transaction));
}
```
You can, of course, also mix your queries as needed.
```c#
private List<Log> GimmeAllTheLogsFromUserIdThatEndsWithTransaction(Guid userId, string transaction)
{
	return Log.SelectList(q => q.UserId == userId && q.Transaction.EndsWith(transaction));
}
```

If you wish to select just one object, you can do the following:
```c#
private Log GimmeTheLogFromUserIdThatEndsWithTransaction(Guid userId, string transaction)
{
	return Log.Select(q => q.UserId == userId && q.Transaction.EndsWith(transaction));
}
```
The method `SelectList` has an overload which accepts an object of type `QueryOptions`, intended to further configure your query with the following parameters:
* `ConnectionToUse`
> Can be null, and if set as such, will default be automatically set to the value you stated in `DefaultConnection`, inside your .config file. If you set a value, make sure it's a connection name that exists inside your .config file.
* `MaximumResults`
> Limits the results of the query. You can set it to -1, which means to bring every record found.
* `Offset`
> Brings back records starting from the specified offset in this property. If set to 0, will simply start from the beginning, as expected ;)

Every query is returned with ordered records. OneData orders them by the descending value of the property marked with `DateCreated` attribute. 
### Intermediate
#### Relationships
We will talk a little about relationships between classes/models inside OneData.

First, let's create a new class/model called `User`:
```c#
using OneData.Attributes;
using OneData.DAO;
using OneData.Interfaces;

[DataTable("users")]
public class User : IManageable
{
    [PrimaryKey]
    public Guid Id { get; set; }
    [DateCreated]
    public DateTime DateCreated { get; set; }
    [DateModified]
    public DateTime DateModified { get; set; }

    public string Name { get; set; }
    public string Lastname { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    public ModelComposition GetComposition()
    {
        return Manager<User>.Composition;
    }
}
```

To achieve a relationship between Users and Logs, we just need to configure it in the `Log` class/model (note the new attribute on `UserId` property):
```c#
[DataTable("logs")]
public class Log : IManageable
{
    [PrimaryKey]
    public Guid Id { get; set; }
    [DateCreated]
    public DateTime DateCreated { get; set; }
    [DateModified]
    public DateTime DateModified { get; set; }

    [ForeignKey(typeof(User))]
    public Guid UserId { get; set; }
    public string Transaction { get; set; }

    public ModelComposition GetComposition()
    {
        return Manager<Log>.Composition;
    }
}
```
And just like that, our classes/models are related! By default, OneData configures the relationship `ON DELETE` and `ON UPDATE` to `NO ACTION`, but you can configure this with the constructor on the attribute.

The following should be used if you need to pull data from another table (related or not) into our class/model for ease of use (note the new property `UserName` and it's attribute).
```c#
[DataTable("logs")]
public class Log : IManageable
{
    [PrimaryKey]
    public Guid Id { get; set; }
    [DateCreated]
    public DateTime DateCreated { get; set; }
    [DateModified]
    public DateTime DateModified { get; set; }

    [ForeignKey(typeof(User))]
    public Guid UserId { get; set; }
    public string Transaction { get; set; }
    [ForeignData(typeof(User))]
    public string UserName { get; set; }

    public ModelComposition GetComposition()
    {
        return Manager<Log>.Composition;
    }
}
```
With this configuration, OneData will look for the `Name` property and get it's value inside our `User` class/model and get data based on the log's `UserId`.

`ForeignData` attribute has three constructors. In the example above, we used the simplest of them but may raise some eyebrows in confusion since it seems so magical at firsts. Next, you will find a detailed explanation:

Just sending the `JoinModel` parameter:
```c#
[ForeignData(typeof(User))]
```
By doing this, OneData has to assume some configurations, which are: 
* Your `ReferenceModel` is the model the property belongs to.
* Your `ReferenceIdName` is using the name of your `JoinModel` plus the word 'Id'.
* Your `ColumnName` is called `Name` inside your `JoinModel`.

Just sending the `JoinModel` and `ColumnName` parameter:
```c#
[ForeignData(typeof(User), nameof(User.Name))]
```
By doing this, OneData has to assume some configurations, which are: 
* Your `ReferenceModel` is the model the property belongs to.
* Your `ReferenceIdName` is using the name of your `JoinModel` plus the word 'Id'.
 
Sending every parameter:
```c#
[ForeignData(typeof(User), typeof(Log), nameof(UserId), nameof(User.Name))]
```
Even tho this seems a bit messy, it's VERY powerful when used on nested properties.

All the magic will be done when you call any transaction of type Select.
#### Generic Stored Procedures
We know that sometimes you need to call a stored procedure that's not a common transaction and for this, you can do the following:
```c#
Manager.StoredProcedure("<your stored procedure name>", <your connection name; null for default>, new Parameter("<parameter name>", <parameter value>));
```

The last parameter in this method accepts an array of type `Parameter`. This means you could just pass a preconfigured array or pass one by one.

#### Async Methods
Every method corresponding to a transaction type can be called using `Async`, although this calls are not as easy as we wish them to be (yet!). Please read the following to understand more.

This is the list of methods available for `Async` calls:
```c#
Manager<T>.InsertAsync(T obj, QueryOptions queryOptions)
Manager<T>.InsertMassiveAsync(IEnumerable<T> list, QueryOptions queryOptions)
Manager<T>.UpdateAsync(T obj, QueryOptions queryOptions)
Manager<T>.UpdateMassiveAsync(IEnumerable<T> list, QueryOptions queryOptions)
Manager<T>.DeleteAsync(T obj, QueryOptions queryOptions)
Manager<T>.DeleteMassiveAsync(IEnumerable<T> list, QueryOptions queryOptions)
Manager<T>.SelectAsync(Expression<Func<T, bool>> expression, QueryOptions queryOptions)
Manager<T>.SelectAllAsync(QueryOptions queryOptions)
Manager.StoredProcedureAsync(string storedProcedure, QueryOptions queryOptions, params Parameter[] parameters)
```
As an example, the following would be used to call an `Async` Insert method to add a new `Log` object into the database:
```c#
await Manager<Log>.InsertAsync(newLog, null);
```
This method's last parameter corresponds to `QueryOptions` object, which can be sent as null to apply the defaults.

## FAQ
### How do I report any kind of issue?
> Please add a new issue in the project's GitHub Issues section. Don't forget to use the proper bug template.
### Can I ask for a new feature?
> Yes! Please! Go ahead and submit your idea using the proper issue template in OneData's GitHub page.
### Do you have any demos?
> We have a demo project, you can find it [here](https://github.com/jorgemk85/OneData.Demo). Please make sure to use the latest OneData's NuGet package.


## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[MIT](https://choosealicense.com/licenses/mit/)

## Icons
[Icons by Icons8](https://icons8.com)
