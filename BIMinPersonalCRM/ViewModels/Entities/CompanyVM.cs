using System.Collections.ObjectModel;
using BIMinPersonalCRM.DataObjects;
using BIMinPersonalCRM.Models;
using BIMinPersonalCRM.MVVM;

namespace BIMinPersonalCRM.ViewModels.Entities
{
    /// <summary>
    ///     Вью-модель компании для использования в представлении.
    /// </summary>
    public class CompanyVM : VMObject
    {
        private readonly CompanyDTO _dto;

        public CompanyVM()
        {
            _dto = new();
            Employees = new();
            Orders = new();
            RelationshipStatus = RelationshipStatus.NotKnown;
            PaymentAbilityStatus = PaymentAbilityStatus.NotKnown;

        }

        public CompanyVM(CompanyDTO dto)
        {
            _dto = dto ?? new CompanyDTO();

            Id = dto.Id;
            Name = dto.Name;
            Phone = dto.Phone;
            Website = dto.Website;
            LogoPath = dto.LogoPath;
            CardColor = dto.CardColor;
            RelationshipStatus = dto.RelationshipStatus;
            Employees = new(dto.Employees.Select(e => new EmployeeVM(e)));
            Orders = new(dto.Orders.Select(o => new OrderVM(o)));
        }

        public int Id
        {
            get => GetValue<int>(nameof(Id));
            set => SetValue(nameof(Id), value);
        }

        public string Name
        {
            get => GetValue<string>(nameof(Name));
            set => SetValue(nameof(Name), value);
        }

        public string Phone
        {
            get => GetValue<string>(nameof(Phone));
            set => SetValue(nameof(Phone), value);
        }

        public string Website
        {
            get => GetValue<string>(nameof(Website));
            set => SetValue(nameof(Website), value);
        }

        public string LogoPath
        {
            get => GetValue<string>(nameof(LogoPath));
            set => SetValue(nameof(LogoPath), value);
        }

        public string CardColor
        {
            get => GetValue<string>(nameof(CardColor));
            set => SetValue(nameof(CardColor), value);
        }

        public RelationshipStatus RelationshipStatus
        {
            get => GetValue<RelationshipStatus>(nameof(RelationshipStatus));
            set => SetValue(nameof(RelationshipStatus), value);
        }

        public PaymentAbilityStatus PaymentAbilityStatus
        {
            get => GetValue<PaymentAbilityStatus>(nameof(PaymentAbilityStatus));
            set => SetValue(nameof(PaymentAbilityStatus), value);
        }

        public ObservableCollection<EmployeeVM> Employees
        {
            get
            {
                var value = GetValue<ObservableCollection<EmployeeVM>>(nameof(Employees));
                if (value == null)
                {
                    value = new();
                    SetValue(nameof(Employees), value);
                }
                return value;
            }
            set => SetValue(nameof(Employees), value);
        }

        public ObservableCollection<OrderVM> Orders
        {
            get
            {
                var value = GetValue<ObservableCollection<OrderVM>>(nameof(Orders));
                if (value == null)
                {
                    value = new();
                    SetValue(nameof(Orders), value);
                }
                return value;
            }
            set
            {
                SetValue(nameof(Orders), value);
                OnPropertyChanged(nameof(AverageProfitability));
            }
        }

        public ProfitabilityStatus AverageProfitability
        {
            get
            {
                if (Orders.Count == 0)
                {
                    return ProfitabilityStatus.NoOrders;
                }

                var average = Orders.Average(order => (int)order.ProfitabilityStatus);
                return (ProfitabilityStatus)Math.Round(average);
            }
        }

        public CompanyDTO ToDto()
        {
            _dto.Id = Id;
            _dto.Name = Name;
            _dto.Phone = Phone;
            _dto.LogoPath = LogoPath;
            _dto.Website = Website;
            _dto.PaymentAbilityStatus = PaymentAbilityStatus;
            _dto.RelationshipStatus = RelationshipStatus;
            _dto.Employees = Employees.Select(e => e.ToDto()).ToList();
            _dto.Orders = Orders.Select(o => o.ToDto()).ToList();
            _dto.CardColor = CardColor;
            return _dto;
        }
    }
}
