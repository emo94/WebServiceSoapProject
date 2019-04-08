using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Components;
using MetroFramework.Forms;
using System.Data.SqlClient;
using System.Configuration;
using Fox.MyServices;

namespace Fox
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        EntityState objState = EntityState.Unchanged;
        public Form1()
        {
            InitializeComponent();
        }

        //Arayüzdeki input itemlerine yazılmış veri varsa bu verileri temizleyen metod
        void ClearInput()
        {
            txtFullName.Text = null;
            txtGSM.Text = null;
            txtDriverKey.Text = null;
            txtDriverID.Text = null;
            txtDepartman.Text = null;
        }

        //Fox modülündeki Form1 yani servisi kullanan Modül açıldığında tetiklenen metod
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                //Modül açılırken ilk iş olarak SOAP Servisten kalıtacağımız bir client nesnesi 
                //oluşturmak bu nesne ile servisimize bağlanabilir ve kullanabiliriz
                DriverServiceSoapClient client = new DriverServiceSoapClient();

                //driverBindingSource SOAP Service'in içinde oluşturduğumuz object tipinde bir nesnedir, bu nesne üzerinden 
                //ekleme silme güncelleme işlemlerimizi yapabilicez
                //NOT : Arayüzü oluştururken dataGridView'e DataGridViewTask üzerinden driverBindingSource nesnesini DataSource değeri olarak verdik
                //      bu sebeple alt satırdaki kod ile driverBindingSource nesnesine aktarılan veriler  dataGridView'de otomatik olarak yüklenir ve arayüzde görünür
                driverBindingSource.DataSource = client.GetAll();

                kapat();
                //Listemiz SOAP Servisteki Insert metoduna gönderilmek üzere hazır, Listemize eklenen son veriyi Insert etme işlemini Save butonunun tıklanmasıyla tetiklenen metoda bırakırız.
            }
            catch (Exception ex)
            {
                MetroFramework.MetroMessageBox.Show(this, ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Arayüzdeki input itemlerinin gerektiği zaman işlevsiz hale gelmesini sağlayan metod
        public void kapat()
        {
            txtDepartman.Enabled = false;
            txtDriverID.Enabled = false;
            txtDriverKey.Enabled = false;
            txtFullName.Enabled = false;
            txtGSM.Enabled = false;
        }

        //Arayüzdeki input itemlerinin gerektiği zaman işlevsel hale gelmesini sağlayan metod
        public void ac()
        {
            txtDepartman.Enabled = true;
            txtDriverID.Enabled = true;
            txtDriverKey.Enabled = true;
            txtFullName.Enabled = true;
            txtGSM.Enabled = true;
        }

        //Ekleme işlemini yapan butonun tıklandığı zaman tetiklenen metod
        private void btnAdd_Click(object sender, EventArgs e)
        {
            //state değerimizi ekleme türüne alırız
            objState = EntityState.Added;
            //driverBindingSource değerinin içinde load metodunda oluşan datalarımızı List<Driver> tipindeki list değişkenimize aktarırız
            List<Driver> list = ((IEnumerable<Driver>)driverBindingSource.DataSource).ToList();
            //Arayüzdeki input değerlerine girdiğimiz veriler Driver sınıfındaki property değerleriyle eşleşmiş durumda olduğu için 
            //alt satırdaki kod ile arayüzümüzdeki input değerlerinin içine girilmiş olan sürücü verisi liste sınıfımızın son elemanı olarak üzerine eklenir
            //son eklenen verinin servise metoduyla kayıt edilmesi için save butonunun click olayı ile tetiklenen 145. satırdaki btnSave_Click metoduna bakabilirsiniz.
            list.Add(new Driver());
            driverBindingSource.DataSource = list.AsEnumerable();
            driverBindingSource.MoveLast();
            txtFullName.Focus();
            ac();
        }

        //Güncelleme işlemini yapan butonun tıklandığı zaman tetiklenen metod
        private void btnEdit_Click(object sender, EventArgs e)
        {
            //state değerimizi güncelleme türüne çevirir
            objState = EntityState.Changed;
            txtFullName.Focus();
            ac();
        }

        //Silme işlemini yapan butonun tıklandığı zaman tetiklenen metod
        private void btnDelete_Click(object sender, EventArgs e)
        {
            objState = EntityState.Deleted;
            if (MetroFramework.MetroMessageBox.Show(this, "Emin misin?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    // datagridview'den son seçilen satırın verilerini obj tutar
                    Driver obj = driverBindingSource.Current as Driver;
                    if (obj != null)
                    {
                        //SOAP Servisten client nesnesini kalıtırız
                        DriverServiceSoapClient client = new DriverServiceSoapClient();
                        //Gönderdiğimiz ID değerine sahip veriyi DB'den silebilirse True silemezse false değer döndüren SOAP servisteki delete metodumuzu çağırırız ve içine obj.DriverID değerini 
                        //parametre olarak göndeririz, dönen değeri result adındaki değişkende tutarız, true dönerse silmiş demektir bu durumda kod if bloğuna girer DB den silinen kayıtı
                        //driverBindingSource nesnesinin içinden de siler
                        bool result = client.Delete(obj.DriverID);
                        if (result)
                        {
                            driverBindingSource.RemoveCurrent();
                            objState = EntityState.Unchanged;
                        }
                    }
                    kapat();

                }
                catch (Exception ex)
                {
                    MetroFramework.MetroMessageBox.Show(this, ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        //Cancel yani iptal işlemini yapan butonun tıklandığı zaman tetiklenen metod
        private void btnCancel_Click(object sender, EventArgs e)
        {
            //işlem sırasında kayıt etmediğimiz işlemi iptal etmek için aşağıdaki kod ile driverBindingSource değerini resetleyerek
            //orjinalde gelen değerlere döndürebiliriz
            driverBindingSource.ResetBindings(false);
            //ClearInput();
            this.Form1_Load(sender, e);
            kapat();
        }

        //Kaydetme işlemini yapan butonun tıklandığı zaman tetiklenen metod
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                groupBox1.Enabled = false;
                driverBindingSource.EndEdit();
                //DataGridiew'de seçili olan satırın verilerini alıp Driver tipinde obj isimli bir nesneye aktarırız
                Driver obj = driverBindingSource.Current as Driver;
                if (obj != null)
                {
                    //SOAP Servisten client nesnesini kalıtırız
                    DriverServiceSoapClient client = new DriverServiceSoapClient();
                    //Eğer yaptığımız son işlem ekleme ise bu if bloğuna girer
                    if (objState == EntityState.Added)
                    {
                        //SOAP Serivismizdeki Insert metodunu kullanarak servis metodu aracılığıyla obj değerimizi DB'ye ekler 
                        obj.DriverID = client.Insert(obj);
                    }
                    //Eğer yaptığımız son işlem güncelleme  ise bu if bloğuna girer
                    else if (objState == EntityState.Changed)
                    {
                        //SOAP Serivismizdeki Update metodunu kullanarak servis metodu aracılığıyla obj değerimizi DB'de günceller
                        client.Update(obj);
                    }
                    //datagridview güncellenir ve güncel verileri yansıtır
                    dataGridView1.Refresh();
                    objState = EntityState.Unchanged;
                }
                kapat();
            }
            catch (Exception ex)
            {
                MetroFramework.MetroMessageBox.Show(this, ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //DataGridView üzerindeki her bir satır veriye tıklandığında tetiklenen metod
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                //datagridviewdeki tıkladğımız her satırın verisini obj nesnesine güncel değer olarak atar
                //ayrıca input itemlerinin properties sekmesinden data bindings özelliğinin altındaki text bölümü üzerinden 
                //datasource olarak driverBindingSource'un ilgili sütun değerlerini atadık ve bu sayede datagridviewde her tıkladğımız satır 
                //hem obj değerine atanıyor hemde input değerlerinin text değerleri oalrak ekranda gözüküyor
                Driver obj = driverBindingSource.Current as Driver;
            }
            catch (Exception ex)
            {
                MetroFramework.MetroMessageBox.Show(this, ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Projeyi kapat butonuna tıklandığında tetiklenen metod
        private void button1_Click(object sender, EventArgs e)
        {
            if (MetroFramework.MetroMessageBox.Show(this, "Emin misin?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}