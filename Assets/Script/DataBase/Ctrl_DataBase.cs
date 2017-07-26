using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SPFW.DataBase
{
	public static class Ctrl_DataBase  
	{
		private static Dictionary<int,IDataBase> _dicDataBase = new Dictionary<int,IDataBase>();

		public static void LoadDataBase()
		{
			_dicDataBase.Clear();

			

			Load();
		}

		private static void Load()
		{
			for(int i=0; i<_dicDataBase.Count; i++)
			{
				IDataBase db = _dicDataBase[i+1];
				db.Load();
			}
		}

		public static T Get<T>() where T : IDataBase, new()
		{
			T pDataBase = new T();

			if(_dicDataBase.ContainsKey(pDataBase.GetDataBaseID()))
			{
				return (T)_dicDataBase[pDataBase.GetDataBaseID()];
			}

			return default(T);
		}

		public static void RegisterDataBase(IDataBase dataBase)
		{
			if(!_dicDataBase.ContainsKey(dataBase.GetDataBaseID()))
			{
				_dicDataBase[dataBase.GetDataBaseID()] = dataBase;
			}
		}
	}
}

