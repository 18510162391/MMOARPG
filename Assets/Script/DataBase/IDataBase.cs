
namespace SPFW.DataBase
{
	public	interface  IDataBase
	{
		int GetDataBaseID();
		string GetDataBasePath();
		void Load();
	}
}
