### 基于MSSQL库生成sql语句，基于Dapper库生成CRUD类
 1. 配置App.config。
 2. 编译程序。
 3. 点击DapperCRUD.exe生成CRUD操作。

## 应用

    public class DimModel
    {
        //主键
    	public int DimId { get; set; }
    
    	public string DimName { get; set; }
    	
    	public int DimTypeId { get; set; }
    }
    
# SELECT

    public DimModel GetDim(int dimId)
    public List<DimModel> GetDim(List<int> dimIds)
    public List<DimModel> GetDim(object param = null)

- param: new { DimTypeId=1 } 

    SQL: SELECT * FROM dbo.DimType WHERE DimTypeId=@DimTypeId;

- param: new { DimTypeId=new List<int>(){1,2,3 } } 

    SQL: SELECT * FROM dbo.DimType WHERE DimTypeId IN @DimTypeId;

- param: new { DimTypeId=new List<int>(){1,2,3 }, DimName="DEMO" } 

    SQL: SELECT * FROM dbo.DimType WHERE DimTypeId IN @DimTypeId AND DimName=@DimName;

# INSERT
- public void Insert(DimModel model)
- public void InsertAsync(DimModel model)
- public void Insert(List<DimModel> models)
- public void InsertAsync(List<DimModel> models)

# UPDATE
-  public void Update(DimModel model)
-  public void UpdateAsync(DimModel model)
-  public void Update(List<DimModel> models)
-  public void UpdateAsync(List<DimModel> models)

# DELETE
- public void DeleteDim(int dimId)
- public void DeleteDimAsync(int dimId)
- public void DeleteDim(List<int> dimIds)
- public void DeleteDimAsync(List<int> dimIds)

# TRUNCATE TABLE
- public void TruncateAsyncDim()
