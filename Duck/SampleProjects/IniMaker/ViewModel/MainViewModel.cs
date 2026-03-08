using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace IniMaker.ViewModel
{
    public enum ESensorNeckDirection
    {
        Top,
        Left,
        Right,
        Bottom
    }

    public enum ESensorNeckAlignment
    {
        Left,
        Center,
        Right
    }

    class MainViewModel : ObservableRecipient
    {
        public List<ESensorNeckDirection> SensorNeckDirections { get; }
        public List<ESensorNeckAlignment> SensorNeckAlignments { get; }

        #region 프로퍼티
        private string mIniName;
        public string IniName
        {
            get
            {
                return mIniName;
            }
            set => SetProperty(ref mIniName, value, true);
        }

        private int mSubsensorCount;
        public int SubsensorCount
        {
            get
            {
                return mSubsensorCount;
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                SetProperty(ref mSubsensorCount, value, true);
            }
        }

        private bool mSubsensorAlignmentType = true;
        public bool SubsensorAlignmentType
        {
            get
            {
                return mSubsensorAlignmentType;
            }
            set
            {
                SetProperty(ref mSubsensorAlignmentType, value, true);
            }
        }

        private double mMaxPressure = 6;
        public double MaxPressure
        {
            get
            {
                return mMaxPressure;
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                if (value < MinPressure)
                {
                    value = MinPressure;
                }
                SetProperty(ref mMaxPressure, value, true);
            }
        }
        private double mMinPressure = 0;
        public double MinPressure
        {
            get
            {
                return mMinPressure;
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                if (value > MaxPressure)
                {
                    value = MaxPressure;
                }
                SetProperty(ref mMinPressure, value, true);
            }
        }

        private ESensorNeckDirection mSelectedSensorNeckDirection;
        public ESensorNeckDirection SelectedSensorNeckDirection
        {
            get
            {
                return mSelectedSensorNeckDirection;
            }
            set => SetProperty(ref mSelectedSensorNeckDirection, value);
        }
        private ESensorNeckAlignment mSelectedSensorNeckAlignment;
        public ESensorNeckAlignment SelectedSensorNeckAlignment
        {
            get
            {
                return mSelectedSensorNeckAlignment;
            }
            set => SetProperty(ref mSelectedSensorNeckAlignment, value);
        }
        #endregion

        #region ICommand
        public ICommand IncreaseSubsensorCommand { get; }
        public ICommand DecreaseSubsensorCommand { get; }
        #endregion

        public MainViewModel()
        {
            #region ViewModel 초기화
            mIniName = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            mSubsensorCount = 0;

            IncreaseSubsensorCommand = new RelayCommand(increaseSubsensor);
            DecreaseSubsensorCommand = new RelayCommand(decreaseSubsensor);

            SensorNeckDirections = new List<ESensorNeckDirection>
            {
                ESensorNeckDirection.Top,
                ESensorNeckDirection.Left,
                ESensorNeckDirection.Right,
                ESensorNeckDirection.Bottom
            };
            SensorNeckAlignments = new List<ESensorNeckAlignment>
            {
                ESensorNeckAlignment.Left,
                ESensorNeckAlignment.Center,
                ESensorNeckAlignment.Right
            };

            PropertyChanged += valueChangedHandler;

            // ObservableRecipient를 상속한 경우 사용할 수 있는 이벤트
            //ErrorsChanged += errorsChangedHandler; // 에러를 핸들링 하는 이벤트가 아닌 에러 상태가 변경되었을 때 UI에 알리기 위한 이벤트
            #endregion

            #region 외부 데이터 로드
            //TODO: [장원덕: 2026-02-22 외부로부터 센서 ini 파일을 읽어와 메모리에 올리는 로직 구현
            #endregion
        }

        private void valueChangedHandler(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IniName))
            {

            }
            else if (e.PropertyName == nameof(SubsensorCount))
            {
                
            }
            else if (e.PropertyName == nameof(MaxPressure))
            {
                
            }
            else if (e.PropertyName == nameof(MinPressure))
            {
                
            }
        }

        private void increaseSubsensor()
        {
            ++SubsensorCount;
        }
        private void decreaseSubsensor()
        {
            if (SubsensorCount > 0)
            {
                --SubsensorCount;
            }
        }

        #region 임시 주석
        // ObservableRecipient 용
        //private void errorsChangedHandler(object? sender, DataErrorsChangedEventArgs e)
        //{
        //    if (e.PropertyName == nameof(IniName))
        //    {
        //    }
        //    else if (e.PropertyName == nameof(SubsensorCount))
        //    {
        //    }
        //    else if (e.PropertyName == nameof(MaxPressure))
        //    {
        //    }
        //    else if (e.PropertyName == nameof(MinPressure))
        //    {
        //    }
        //}
        #endregion
    }
}
