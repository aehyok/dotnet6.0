## .net 8

- **1、定时任务**

- **2、RabbitMQ**

- **3、Redis持久化缓存**

- **4、Mysql数据库**

- **5、Z.EntityFramework.Extensions.EFCore不开源**
 - https://entityframework-extensions.net
 - https://github.com/zzzprojects/EntityFramework-Extensions

- **6、EFCore更新数据库**
  ```
   在aehyok.Schedules项目下

   dotnet-ef migrations add InitTask -c DvsContext --framework net8.0 -v
   
   dotnet-ef database update -c DvsContext --framework net8.0 -v
  ```  

## 使用的开源库

```
- DDD 
    - https://github.com/ntxinh/AspNetCore-DDD
- EFCore查询 
    - https://github.com/ardalis/Specification
    - https://specification.ardalis.com/getting-started
- EFCore QueryRepository RepositoryBase
    - https://github.com/TanvirArjel/EFCore.GenericRepository
- PaginatedList 数据分页
    - https://github.com/dncuug/X.PagedList    
- LinqKit 
    - https://github.com/scottksmith95/LINQKit
- AutoMapper
    - https://automapper.org
    - https://github.com/AutoMapper/AutoMapper    
- RabbitMQ 
```