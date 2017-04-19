using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace ImportData.Helpers
{
    public class EntityBase
    {
    }

    public class CustomEntity : EntityBase, INotifyPropertyChanged
    {
        public CustomEntity()
            : base()
        {
            this.Properties = new ObservableCollection<object>();
            this.FuncValues = new ObservableCollection<object>();
            ListErrors = new List<CustomEntityError>();
            Properties.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Properties_CollectionChanged);
        }

        void Properties_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                int n1 = ListErrors.Count;
                int n2 = Properties.Count;
                for (int i = n1; i < n2; i++)
                {
                    ListErrors.Add(null);
                    FuncValues.Add(null);
                }
            }
        }

        #region RefreshError
        public delegate void RefreshError(CustomEntity sender);
        private event RefreshError _NeedRefreshError = null;
        public event RefreshError NeedRefreshError
        {
            add
            {
                _NeedRefreshError += value;
            }
            remove
            {
                _NeedRefreshError -= value;
            }
        }
        public void RaiseEventNeedRefreshError()
        {
            if (_NeedRefreshError != null)
                _NeedRefreshError(this);
        }
        #endregion

        #region Error
        //public Action<CustomEntity> RefreshError;        
        public bool HasError
        {
            get
            {
                return Errors.Any(a => a != null && a.ErrorTypeEnum == ErrorTypeEnum.Error);
            }
        }

        public bool HasWarning
        {
            get
            {
                return Errors.Any(a => a != null && a.ErrorTypeEnum == ErrorTypeEnum.Warning);
            }
        }
        /// <summary>
        /// object: index of property
        /// Can not use Value. because NULL can duplicate
        /// </summary>
        public void SetError(int propertyIndex, CustomEntityError error)
        {
            if (Properties == null)
                return;
            if (Properties.Count <= propertyIndex)
                return;
            ListErrors[propertyIndex] = error;
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Errors"));
        }
        /// <summary>
        /// Không được set thuoc tính này. 
        /// This use for binding  error
        /// </summary>
        private List<CustomEntityError> ListErrors { get; set; }
        public CustomEntityError[] Errors
        { get { return ListErrors.ToArray(); } }


        public static string GetPropertyPath_ErrorBackground(int index)
        {
            return string.Format("Errors[{0}].ColorDisplayWhenError", index);
        }
        public static string GetPropertyPath_ErrorTooltip(int index)
        {
            return string.Format("Errors[{0}].Tooltip", index);
        }
        public static string GetPropertyPath_Error
        {
            get
            {
                return "HasError";
            }
        }
        public static string GetPropertyPath_Warning
        {
            get
            {
                return "HasWarning";
            }
        }
        #endregion

        public ObservableCollection<object> Properties { get; protected set; }

        /// <summary>
        /// The real values that used to import to DB
        /// </summary>
        public ObservableCollection<object> FuncValues { get; protected set; }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public int? UniqueColumnIndex = null;

        public static string GetPropertyName(int index)
        {
            return string.Format("Properties[{0}]", index);
        }

        public static string GetFuncValueName(int index)
        {
            return string.Format("FuncValues[{0}]", index);
        }

        public static string GetBackgroundFieldName(int index)
        {
            return string.Format("Backgrounds[{0}]", index);
        }
    }

    public class CustomEntityError
    {
        public ErrorTypeEnum ErrorTypeEnum { get; set; }
        public Brush ColorDisplayWhenError { get; set; }
        public string Tooltip { get; set; }
    }

    public enum ErrorTypeEnum
    {
        Nothing,
        Warning,
        Error
    }
}
