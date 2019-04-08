using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Dapper;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;

namespace MyServices
{
    /// <summary>
    /// Summary description for DriverService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class DriverService : System.Web.Services.WebService
    {
        //driverBindingSource değişkeni Serivis kullanacak olan Modül ile Servis arasında data bağlantısı için oluşturuldu
        public object driverBindingSource { get; private set; }


        [WebMethod(Description = @"Veri tabanına ekleme işlemini yapan SOAP service metodudur")]
        public int Insert(Driver obj)
        {
            //Ekleme işleminin yapılacağı veri tabanına bağlanabilmek için bağlantı bilgilerini içeren cn isimli değer, web configten çekilir ve db adındaki 
            //IDbConnection tipindeki interfacemize aktarılır artık veri tabanına db değişkeni ile erişebileceğiz
            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["cn"].ConnectionString))
            {
                //veri tabanı bağlantısı kapalı ise aç
                if (db.State == ConnectionState.Closed)
                {
                    db.Open();
                }
                //DynamicParameters tipindekiş p adında bir nesne oluştururuz
                DynamicParameters p = new DynamicParameters();
                //birazdan veri tabanına ekleme işlemlerini yapmak için kullanacağımız Store Procedure'de parametre olarak kullanacağımız propertyleri
                //aynı özellikler ile p nesnemize de ekliyoruz
                //DriverID değeri veri tabanımızda int tipinde ve primaryKey görevi görüyor bu özellikleri p ye parametre olarak dinamik bir şekilde 
                //yani DynamicParameters mantığıyla eklemiş olduk
                p.Add("@DriverID", dbType: DbType.Int32, direction: ParameterDirection.Output);
                //veri tabanımızdaki diğer sütun tiplerini de p nesnemize parametre olarak ekliyoruz
                p.AddDynamicParams(new { DriverName = obj.DriverName, DriverGSM = obj.DriverGSM, DriverKey = obj.DriverKey, DriverDepartment = obj.DriverDepartment });
                //ve veri ekleme store proceduremizi p nesnemizin parametrelerini göndererek tetikliyoruz
                int result = db.Execute("sp_Driver_Insert_Test", p, commandType: CommandType.StoredProcedure);
                if (result!=0)
                {
                    return p.Get<int>("@DriverID");
                }

                return 0;
            }
        }

        [WebMethod(Description = @"Veri tabanındaki verileri listeleme işlemini yapan SOAP service metodudur")]
        public List<Driver> GetAll()
        {
            //Sorgulama işleminin yapılacağı veri tabanına bağlanabilmek için bağlantı bilgilerini içeren cn isimli değer, web configten çekilir ve db adındaki 
            //IDbConnection tipindeki interfacemize aktarılır artık veri tabanına db değişkeni ile erişebileceğiz
            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["cn"].ConnectionString))
            {
                //veri tabanı bağlantısı kapalı ise aç
                if (db.State == ConnectionState.Closed)
                { db.Open(); }
                //Dapper.dll kütüphanesinden gelen Query metodu ile aşağıdaki tek satır kodun basitliği ve kolaylığıyla veri tabanımıza sorgu gönderirirz,
                //ve sorgu sonucu gelen verileri List<Driver> yani Driver propertyleri olarak liste tipinde return değeri olarak geri döndürürüz
                return db.Query<Driver>("select DriverID, DriverName, DriverGSM, DriverKey, DriverDepartment from Driver", commandType: CommandType.Text).ToList();
            }
        }

        [WebMethod(Description = @"Veri tabanında istenilen veriyi silme işlemini yapan SOAP service metodudur")]
        public bool Delete(int driverID)
        {
            //Silme işleminin yapılacağı veri tabanına bağlanabilmek için bağlantı bilgilerini içeren cn isimli değer, web configten çekilir ve db adındaki 
            //IDbConnection tipindeki interfacemize aktarılır artık veri tabanına db değişkeni ile erişebileceğiz
            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["cn"].ConnectionString))
            {
                //veri tabanı bağlantısı kapalı ise aç
                if (db.State == ConnectionState.Closed)
                {
                    db.Open();
                }
                //Dapper.dll kütüphanesinden gelen Execute metodu ile aşağıdaki tek satır kodun basitliği ve kolaylığıyla veri tabanımızda ekleme silme ve güncelleme komutu gönderebiliriz
                //dilersek doğrudan SQL sorgusu ile işlem yapılabilir(alt satırda olduğu gibi), dilersekte Store Procedure çağırılabilir (satır : 108)
                int result = db.Execute("delete from Driver where DriverID= @DriverID", new { DriverID = driverID }, commandType: CommandType.Text);
                return result != 0;
            }
        }

        [WebMethod(Description = @"Veri tabanında istenilen veriye güncelleme işlemini yapan SOAP service metodudur")]
        public bool Update(Driver obj)
        {
            //Güncelleme işleminin yapılacağı veri tabanına bağlanabilmek için bağlantı bilgilerini içeren cn isimli değer, web configten çekilir ve db adındaki 
            //IDbConnection tipindeki interfacemize aktarılır artık veri tabanına db değişkeni ile erişebileceğiz
            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["cn"].ConnectionString))
            {
                //veri tabanı bağlantısı kapalı ise aç
                if (db.State == ConnectionState.Closed)
                {
                    db.Open();
                }
                //Dapper.dll kütüphanesinden gelen Execute metodu ile aşağıdaki tek satır kodun basitliği ve kolaylığıyla veri tabanımızda ekleme silme ve güncelleme komutu gönderebiliriz
                //Execute metoduyla veritabanına SQL sorgusu göndermek yerine, veri tabanında bulunan sp_Driver_Update_Test isimli procedure'yi  varsa parametre değerlerini vererek tetikleyebiliriz.
                int result = db.Execute("sp_Driver_Update_Test", new { DriverID = obj.DriverID, DriverName = obj.DriverName, DriverGSM = obj.DriverGSM, DriverKey = obj.DriverKey, DriverDepartment = obj.DriverDepartment }, commandType: CommandType.StoredProcedure);
                return result != 0;
            }
        }
    }
}
