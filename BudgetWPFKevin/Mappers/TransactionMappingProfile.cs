using AutoMapper;
using BudgetWPFKevin.Models;
using BudgetWPFKevin.ViewModels.Transactions;

namespace BudgetWPFKevin.Mappers
{
    public class TransactionMappingProfile : Profile
    {

        // Mappings mellan vymodeller och datamodeller för transaktion

        public TransactionMappingProfile()
        {
            CreateMap<TransactionItemViewModel, Transaction>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.RecurringTransactionId, opt => opt.MapFrom(src => src.RecurringTransactionId))
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.RecurringTransaction, opt => opt.Ignore());

            CreateMap<RecurringTransactionItemVM, RecurringTransaction>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.Month, opt => opt.MapFrom(src => src.RecurrenceMonth))
                .ForMember(dest => dest.RecurrenceType, opt => opt.MapFrom(src => src.RecurrenceType))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src =>
                    src.EndDate.HasValue ? new DateTimeOffset(src.EndDate.Value) : (DateTimeOffset?)null))
                .ForMember(dest => dest.IsRecurring, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.IsSystemGenerated, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore());

            CreateMap<RecurringTransaction, Transaction>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) 
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.RecurringTransactionId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Date, opt => opt.Ignore()) 
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.RecurringTransaction, opt => opt.Ignore());

            CreateMap<TransactionItemViewModel, RecurringTransaction>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) 
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.Date))
                
      
                .ForMember(dest => dest.IsRecurring, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.IsSystemGenerated, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore());

            CreateMap<RecurringTransactionItemVM, Transaction>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) 
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.StartDate)) 
            .ForMember(dest => dest.RecurringTransactionId, opt => opt.Ignore()) 
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.RecurringTransaction, opt => opt.Ignore());
        }
    }
}