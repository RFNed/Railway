using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using RailwayApp.Models;
using RailwayApp.Services;

namespace RailwayApp.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;

    public ObservableCollection<TrainDetails> Trains { get; } = [];
    public ObservableCollection<Employee> Employees { get; } = [];
    public string[] JobTitles { get; } = ["manager", "driver", "assistant"];
    public ObservableCollection<LookupItem> Cities { get; } = [];
    public ObservableCollection<LookupItem> Managers { get; } = [];
    public ObservableCollection<LookupItem> Drivers { get; } = [];
    public ObservableCollection<LookupItem> Assistants { get; } = [];
    public ObservableCollection<Locomotive> Locomotives { get; } = [];
    public ObservableCollection<WagonWithType> Wagons { get; } = [];
    public ObservableCollection<WagonType> WagonTypes { get; } = [];
    public ObservableCollection<Locomotive> FreeLocomotives { get; } = [];
    public ObservableCollection<WagonWithType> FreeWagons { get; } = [];
    public ObservableCollection<WagonCompositionItem> CompositionWagons { get; } = [];
    public ObservableCollection<Locomotive> AssignedLocomotives { get; } = [];

    [ObservableProperty] private TrainDetails? _selectedTrain;
    [ObservableProperty] private Employee? _selectedEmployee;
    [ObservableProperty] private Locomotive? _selectedLocomotive;
    [ObservableProperty] private WagonWithType? _selectedWagon;
    [ObservableProperty] private WagonCompositionItem? _selectedCompositionWagon;
    [ObservableProperty] private Locomotive? _selectedFreeLocomotive;
    [ObservableProperty] private WagonWithType? _selectedFreeWagon;
    [ObservableProperty] private bool _newTrainMode;
    [ObservableProperty] private bool _newEmployeeMode;
    [ObservableProperty] private string _editNumber = string.Empty;
    [ObservableProperty] private string _editFormation = string.Empty;
    [ObservableProperty] private LookupItem? _selectedDepCity;
    [ObservableProperty] private LookupItem? _selectedArrCity;
    [ObservableProperty] private LookupItem? _selectedManager;
    [ObservableProperty] private LookupItem? _selectedDriver;
    [ObservableProperty] private LookupItem? _selectedAssistant;
    [ObservableProperty] private string _lastName = string.Empty;
    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private string _middleName = string.Empty;
    [ObservableProperty] private string _birthDate = string.Empty;
    [ObservableProperty] private string _phone = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _jobTitle = string.Empty;
    [ObservableProperty] private string _editLocomotiveNumber = string.Empty;
    [ObservableProperty] private string _editWagonNumber = string.Empty;
    [ObservableProperty] private WagonType? _selectedWagonType;
    [ObservableProperty] private string _reportFrom = DateTime.Today.AddMonths(-1).ToString("dd.MM.yyyy");
    [ObservableProperty] private string _reportTo = DateTime.Today.ToString("dd.MM.yyyy");
    [ObservableProperty] private string _reportManagerResult = "Введите период и выберите менеджера.";
    [ObservableProperty] private string _wagonStatistics = "Выберите состав.";
    [ObservableProperty] private IBrush _statusBackground = new SolidColorBrush(Color.Parse("#405C73"));
    [ObservableProperty] private string _statusMessage = "Готово";

    public MainWindowViewModel()
    {
        var config = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json", false, true).Build();
        string connectionString = config.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Строка подключения DefaultConnection не найдена.");
        _dbService = new DatabaseService(connectionString);
        _ = LoadDataAsync();
    }

    partial void OnSelectedTrainChanged(TrainDetails? value) { if (value != null) _ = SelectTrainAsync(value); }
    partial void OnSelectedEmployeeChanged(Employee? value) => LoadEmployee(value);

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            SetStatus("Загрузка данных...", "#405C73");
            var cities = await _dbService.GetCitiesAsync(); Replace(Cities, cities.Select(x => new LookupItem { Id=x.CityId, Name=x.CityName }));
            await _dbService.RecalculateEmployeeRatingsAsync(); Replace(Managers, await _dbService.GetEmployeesByRoleAsync("manager")); Replace(Drivers, await _dbService.GetEmployeesByRoleAsync("driver")); Replace(Assistants, await _dbService.GetEmployeesByRoleAsync("assistant"));
            Replace(Employees, await _dbService.GetEmployeesAsync()); Replace(Locomotives, await _dbService.GetLocomotivesAsync()); Replace(Wagons, await _dbService.GetWagonsAsync()); Replace(WagonTypes, await _dbService.GetWagonTypesAsync());
            var trains=await _dbService.GetTrainsAsync(); Trains.Clear(); foreach(var train in trains) { train.CompositionNodes=await BuildCompositionNodesAsync(train.TrainId); Trains.Add(train); }
            SetStatus("Данные успешно загружены.", "#405C73");
        }
        catch(Exception ex) { SetStatus($"Ошибка подключения или БД: {ex.Message}", "#A52A2A"); }
    }

    private async Task SelectTrainAsync(TrainDetails train)
    {
        LoadTrainEditor(train);
        try { train.CompositionNodes=await BuildCompositionNodesAsync(train.TrainId); var composition=await _dbService.GetCompositionAsync(train.TrainId); Replace(AssignedLocomotives, composition.Locomotives); var loadStats=TrainAlgorithms.CalculateWagonLoadStatistics(composition.TrainWagons.Select(x=>x.IsLoaded)); WagonStatistics=$"Загружено: {loadStats.LoadedCount}; пустых: {loadStats.EmptyCount}; загружено: {loadStats.LoadedPercent:0.##}%; пустых: {loadStats.EmptyPercent:0.##}%."; CompositionWagons.Clear(); for(int i=0;i<composition.TrainWagons.Count;i++){var tw=composition.TrainWagons[i];var w=composition.Wagons.FirstOrDefault(x=>x.WagonId==tw.WagonId);if(w!=null)CompositionWagons.Add(new WagonCompositionItem{TrainWagonId=tw.TrainWagonId,WagonId=w.WagonId,WagonNumber=w.WagonNumber,WagonTypeCode=w.WagonTypeCode,TypeDescription=w.TypeDescription,IsLoaded=tw.IsLoaded});} Replace(FreeLocomotives,await _dbService.GetAllFreeLocomotivesAsync(train.TrainId)); Replace(FreeWagons,await _dbService.GetAllFreeWagonsAsync(train.TrainId)); } catch(Exception ex){SetStatus($"Не удалось загрузить состав: {ex.Message}","#A52A2A");}
    }

    private void LoadTrainEditor(TrainDetails train)
    {
        NewTrainMode=false; EditNumber=train.TrainNumber; EditFormation=train.FormationDatetime.ToString("dd.MM.yyyy HH:mm"); SelectedDepCity=Cities.FirstOrDefault(x=>x.Id==train.DepartureCityId); SelectedArrCity=Cities.FirstOrDefault(x=>x.Id==train.ArrivalCityId); SelectedManager=Managers.FirstOrDefault(x=>x.Id==train.ManagerId); SelectedDriver=Drivers.FirstOrDefault(x=>x.Id==train.DriverId); SelectedAssistant=Assistants.FirstOrDefault(x=>x.Id==train.AssistantId);
    }

    [RelayCommand]
    private void NewTrain() { NewTrainMode=true; SelectedTrain=null; EditNumber=""; EditFormation=DateTime.Now.ToString("dd.MM.yyyy HH:mm"); SelectedDepCity=Cities.FirstOrDefault(); SelectedArrCity=Cities.Skip(1).FirstOrDefault(); SelectedManager=Managers.FirstOrDefault(); SelectedDriver=Drivers.FirstOrDefault(); SelectedAssistant=Assistants.FirstOrDefault(); SetStatus("Введите данные нового состава.","#405C73"); }

    [RelayCommand]
    private async Task SaveTrainAsync()
    {
        if(string.IsNullOrWhiteSpace(EditNumber)||SelectedDepCity==null||SelectedArrCity==null||SelectedManager==null||SelectedDriver==null||SelectedAssistant==null){SetStatus("Ошибка: заполните номер, пункты и всех ответственных сотрудников.","#A52A2A");return;}
        if(SelectedDepCity.Id==SelectedArrCity.Id){SetStatus("Ошибка: пункт отправления и пункт назначения должны различаться.","#A52A2A");return;}
        if(!DateTime.TryParse(EditFormation,out var formation)){SetStatus("Ошибка даты: используйте формат ДД.ММ.ГГГГ ЧЧ:ММ.","#A52A2A");return;}
        try{bool ok;if(NewTrainMode){ok=await _dbService.CreateTrainAsync(new NewTrain{TrainNumber=EditNumber.Trim(),DepartureCityId=SelectedDepCity.Id,ArrivalCityId=SelectedArrCity.Id,FormationDatetime=formation,ManagerId=SelectedManager.Id,DriverId=SelectedDriver.Id,AssistantId=SelectedAssistant.Id});}else if(SelectedTrain!=null){SelectedTrain.TrainNumber=EditNumber.Trim();SelectedTrain.DepartureCityId=SelectedDepCity.Id;SelectedTrain.ArrivalCityId=SelectedArrCity.Id;SelectedTrain.FormationDatetime=formation;SelectedTrain.ManagerId=SelectedManager.Id;SelectedTrain.DriverId=SelectedDriver.Id;SelectedTrain.AssistantId=SelectedAssistant.Id;ok=await _dbService.UpdateTrainAsync(SelectedTrain);}else{SetStatus("Выберите состав или нажмите «Новый состав».","#A52A2A");return;} if(ok){SetStatus(NewTrainMode?"Состав создан.":"Состав обновлён.","#405C73");NewTrainMode=false;await LoadDataAsync();}else SetStatus("Запись не была изменена.","#A52A2A");}catch(Exception ex){SetStatus($"Ошибка сохранения: {ex.Message}","#A52A2A");}
    }

    [RelayCommand]
    private async Task DeleteTrainAsync() { if(SelectedTrain==null){SetStatus("Выберите состав для удаления.","#A52A2A");return;} try{if(await _dbService.DeleteTrainAsync(SelectedTrain.TrainId)){SetStatus("Состав удалён.","#405C73");SelectedTrain=null;await LoadDataAsync();}}catch(Exception ex){SetStatus($"Удаление запрещено или не удалось: {ex.Message}","#A52A2A");} }

    [RelayCommand]
    private void NewLocomotive(){SelectedLocomotive=null;EditLocomotiveNumber=string.Empty;SetStatus("Введите номер нового локомотива.","#405C73");}

    [RelayCommand]
    private void EditLocomotive(){if(SelectedLocomotive==null){SetStatus("Выберите локомотив в справочнике.","#A52A2A");return;}EditLocomotiveNumber=SelectedLocomotive.LocomotiveNumber;}

    [RelayCommand]
    private async Task SaveLocomotiveAsync(){if(string.IsNullOrWhiteSpace(EditLocomotiveNumber)){SetStatus("Введите номер локомотива.","#A52A2A");return;}try{bool ok=SelectedLocomotive==null?await _dbService.CreateLocomotiveAsync(EditLocomotiveNumber.Trim()):await _dbService.UpdateLocomotiveAsync(SelectedLocomotive.LocomotiveId,EditLocomotiveNumber.Trim());SetStatus(ok?"Локомотив сохранён.":"Запись не изменена.",ok?"#405C73":"#A52A2A");EditLocomotiveNumber="";SelectedLocomotive=null;await LoadDataAsync();}catch(Exception ex){SetStatus($"Ошибка локомотива: {ex.Message}","#A52A2A");}}

    [RelayCommand]
    private async Task DeleteLocomotiveAsync(){if(SelectedLocomotive==null){SetStatus("Выберите локомотив для удаления.","#A52A2A");return;}try{await _dbService.DeleteLocomotiveAsync(SelectedLocomotive.LocomotiveId);SelectedLocomotive=null;await LoadDataAsync();SetStatus("Локомотив удалён.","#405C73");}catch(Exception ex){SetStatus($"Удаление локомотива запрещено: {ex.Message}","#A52A2A");}}

    [RelayCommand]
    private async Task AddLocomotiveAsync(){if(SelectedTrain==null||SelectedFreeLocomotive==null){SetStatus("Выберите состав и локомотив.","#A52A2A");return;}try{await _dbService.AddLocomotiveToTrainAsync(SelectedTrain.TrainId,SelectedFreeLocomotive.LocomotiveId);await SelectTrainAsync(SelectedTrain);SetStatus("Локомотив добавлен в состав.","#405C73");}catch(Exception ex){SetStatus(ex.Message,"#A52A2A");}}

    [RelayCommand]
    private async Task RemoveLocomotiveAsync(){if(SelectedTrain==null||SelectedLocomotive==null){SetStatus("Выберите назначенный локомотив.","#A52A2A");return;}try{var composition=await _dbService.GetCompositionAsync(SelectedTrain.TrainId);var link=composition.Locomotives.FirstOrDefault(x=>x.LocomotiveId==SelectedLocomotive.LocomotiveId);if(link==null){SetStatus("Локомотив не найден в составе.","#A52A2A");return;}await _dbService.RemoveLocomotiveFromTrainAsync(await _dbService.GetTrainLocomotiveIdAsync(SelectedTrain.TrainId,SelectedLocomotive.LocomotiveId));await SelectTrainAsync(SelectedTrain);SetStatus("Локомотив удалён из состава.","#405C73");}catch(Exception ex){SetStatus($"Ошибка удаления локомотива: {ex.Message}","#A52A2A");}}

    [RelayCommand]
    private void NewWagon(){SelectedWagon=null;EditWagonNumber=string.Empty;SelectedWagonType=WagonTypes.FirstOrDefault();SetStatus("Введите данные нового вагона.","#405C73");}

    [RelayCommand]
    private async Task SaveWagonAsync(){if(string.IsNullOrWhiteSpace(EditWagonNumber)||SelectedWagonType==null){SetStatus("Введите номер вагона и выберите тип.","#A52A2A");return;}try{bool ok=SelectedWagon==null?await _dbService.CreateWagonAsync(EditWagonNumber.Trim(),SelectedWagonType.WagonTypeId):await _dbService.UpdateWagonAsync(SelectedWagon.WagonId,EditWagonNumber.Trim(),SelectedWagonType.WagonTypeId);SetStatus(ok?"Вагон сохранён.":"Запись не изменена.",ok?"#405C73":"#A52A2A");EditWagonNumber="";SelectedWagon=null;await LoadDataAsync();}catch(Exception ex){SetStatus($"Ошибка вагона: {ex.Message}","#A52A2A");}}

    [RelayCommand]
    private void EditWagon(){if(SelectedWagon==null){SetStatus("Выберите вагон в справочнике.","#A52A2A");return;}EditWagonNumber=SelectedWagon.WagonNumber;SelectedWagonType=WagonTypes.FirstOrDefault(x=>x.WagonTypeId==SelectedWagon.WagonTypeId);}

    [RelayCommand]
    private async Task DeleteWagonAsync(){if(SelectedWagon==null){SetStatus("Выберите вагон для удаления.","#A52A2A");return;}try{await _dbService.DeleteWagonAsync(SelectedWagon.WagonId);SelectedWagon=null;await LoadDataAsync();SetStatus("Вагон удалён.","#405C73");}catch(Exception ex){SetStatus($"Удаление вагона запрещено: {ex.Message}","#A52A2A");}}

    [RelayCommand]
    private async Task AddWagonAsync(){if(SelectedTrain==null||SelectedFreeWagon==null){SetStatus("Выберите состав и вагон.","#A52A2A");return;}try{await _dbService.AddWagonToTrainAsync(SelectedTrain.TrainId,SelectedFreeWagon.WagonId,false);await SelectTrainAsync(SelectedTrain);SetStatus("Вагон добавлен. По умолчанию он пустой.","#405C73");}catch(Exception ex){SetStatus(ex.Message,"#A52A2A");}}

    [RelayCommand]
    private async Task ToggleWagonAsync(WagonCompositionItem? item){if(item==null)return;try{item.IsLoaded=!item.IsLoaded;await _dbService.UpdateTrainWagonLoadAsync(item.TrainWagonId,item.IsLoaded);SetStatus("Признак загрузки вагона изменён.","#405C73");if(SelectedTrain!=null)SelectedTrain.CompositionNodes=await BuildCompositionNodesAsync(SelectedTrain.TrainId);}catch(Exception ex){item.IsLoaded=!item.IsLoaded;SetStatus($"Ошибка изменения вагона: {ex.Message}","#A52A2A");}}

    [RelayCommand]
    private async Task RemoveWagonAsync(WagonCompositionItem? item){if(item==null){SetStatus("Выберите вагон в составе.","#A52A2A");return;}try{await _dbService.RemoveWagonFromTrainAsync(item.TrainWagonId);if(SelectedTrain!=null)await SelectTrainAsync(SelectedTrain);SetStatus("Вагон удалён из состава.","#405C73");}catch(Exception ex){SetStatus($"Ошибка удаления вагона: {ex.Message}","#A52A2A");}}

    [RelayCommand]
    private async Task ToggleSelectedWagonAsync(){await ToggleWagonAsync(SelectedCompositionWagon);}

    [RelayCommand]
    private async Task RemoveSelectedWagonAsync(){await RemoveWagonAsync(SelectedCompositionWagon);}

    [RelayCommand]
    private void NewEmployee(){NewEmployeeMode=true;SelectedEmployee=null;LastName=FirstName=MiddleName=Phone=Email=JobTitle=string.Empty;BirthDate="01.01.1980";SetStatus("Введите данные нового сотрудника.","#405C73");}

    private void LoadEmployee(Employee? employee){if(employee==null)return;NewEmployeeMode=false;LastName=employee.LastName;FirstName=employee.FirstName;MiddleName=employee.MiddleName;BirthDate=employee.BirthDate.ToString("dd.MM.yyyy");Phone=employee.Phone;Email=employee.Email;JobTitle=employee.JobTitle;}

    [RelayCommand]
    private async Task SaveEmployeeAsync(){if(!ValidateEmployee(out var birthDate,out var warning))return;if(NewEmployeeMode){try{await _dbService.CreateEmployeeAsync(new NewEmployee{LastName=LastName.Trim(),FirstName=FirstName.Trim(),MiddleName=MiddleName.Trim(),BirthDate=birthDate,Phone=Phone.Trim(),Email=Email.Trim(),JobTitle=JobTitle});SetStatus(warning??"Сотрудник создан.",warning==null?"#405C73":"#C58B00");}catch(Exception ex){SetStatus($"Ошибка создания: {ex.Message}","#A52A2A");return;}}else if(SelectedEmployee!=null){try{await _dbService.UpdateEmployeeAsync(SelectedEmployee.EmployeeId,new UpdateEmployee{LastName=LastName.Trim(),FirstName=FirstName.Trim(),MiddleName=MiddleName.Trim(),BirthDate=birthDate,Phone=Phone.Trim(),Email=Email.Trim(),JobTitle=JobTitle,Rating=SelectedEmployee.Rating});SetStatus(warning??"Сотрудник обновлён.",warning==null?"#405C73":"#C58B00");}catch(Exception ex){SetStatus($"Ошибка изменения: {ex.Message}","#A52A2A");return;}}else{SetStatus("Выберите сотрудника или нажмите «Новый сотрудник».","#A52A2A");return;}await LoadDataAsync();}

    private bool ValidateEmployee(out DateOnly birthDate,out string? warning){birthDate=default;warning=null;if(string.IsNullOrWhiteSpace(LastName)||string.IsNullOrWhiteSpace(FirstName)){SetStatus("Ошибка: фамилия и имя обязательны.","#A52A2A");return false;}if(!DateOnly.TryParse(BirthDate,out birthDate)){SetStatus("Ошибка даты рождения: используйте формат ДД.ММ.ГГГГ.","#A52A2A");return false;}if(birthDate.Year<1960||birthDate.Year>2005){SetStatus("Ошибка: дата рождения должна быть не ранее 1960 и не позднее 2005 года.","#A52A2A");return false;}if(!System.Text.RegularExpressions.Regex.IsMatch(Phone.Trim(),@"^\+7 \d{3} \d{3} \d{2} \d{2}$")){SetStatus("Ошибка телефона: используйте маску +7 XXX XXX XX XX.","#A52A2A");return false;}if(string.IsNullOrWhiteSpace(JobTitle)){SetStatus("Ошибка «Должность не выбрана»: выберите менеджера, машиниста или помощника.","#A52A2A");return false;}if(string.IsNullOrWhiteSpace(MiddleName))warning="Предупреждение: отчество не указано.";return true;}

    [RelayCommand]
    private async Task DeleteEmployeeAsync(){if(SelectedEmployee==null){SetStatus("Выберите сотрудника для удаления.","#A52A2A");return;}try{await _dbService.DeleteEmployeeAsync(SelectedEmployee.EmployeeId);SelectedEmployee=null;await LoadDataAsync();SetStatus("Сотрудник удалён.","#405C73");}catch(Exception ex){SetStatus($"Удаление запрещено: сотрудник используется в составах. {ex.Message}","#A52A2A");}}

    [RelayCommand]
    private async Task CalculateReportAsync(){if(SelectedManager==null){SetStatus("Выберите менеджера для отчёта.","#A52A2A");return;}if(!DateTime.TryParse(ReportFrom,out var from)||!DateTime.TryParse(ReportTo,out var to)){SetStatus("Ошибка периода: используйте ДД.ММ.ГГГГ.","#A52A2A");return;}to=to.Date.AddDays(1).AddTicks(-1);if(from>to){SetStatus("Ошибка: начало периода позже конца.","#A52A2A");return;}try{var data=await _dbService.GetManagerStatisticsAsync(SelectedManager.Id,from,to);var stats=TrainAlgorithms.CalculateManagerStatistics(data.ManagerTrainCount,data.TotalTrainCount);ReportManagerResult=$"Менеджер: {SelectedManager.Name}\nСформировано за период: {stats.ManagerTrainCount}\nВсего составов за период: {stats.TotalTrainCount}\nДоля: {stats.ManagerPercent:0.##}%";SetStatus("Отчёт рассчитан.","#405C73");}catch(Exception ex){SetStatus($"Ошибка отчёта: {ex.Message}","#A52A2A");}}

    private async Task<ObservableCollection<CompositionNode>> BuildCompositionNodesAsync(int trainId){var composition=await _dbService.GetCompositionAsync(trainId);var nodes=new ObservableCollection<CompositionNode>();var locoNode=new CompositionNode{Title=$"Локомотивы ({composition.Locomotives.Count}/2)"};foreach(var l in composition.Locomotives)locoNode.Children.Add(new CompositionNode{Title=l.LocomotiveNumber,Details=$"Локомотив № {l.LocomotiveNumber} | ID {l.LocomotiveId}"});nodes.Add(locoNode);var wagonNode=new CompositionNode{Title=$"Вагоны ({composition.Wagons.Count})"};for(int i=0;i<composition.Wagons.Count;i++){var w=composition.Wagons[i];var tw=composition.TrainWagons[i];wagonNode.Children.Add(new CompositionNode{Title=$"{w.WagonNumber} — {w.WagonTypeCode}",Details=$"{w.TypeDescription} | {(tw.IsLoaded?"загружен":"пустой")}"});}nodes.Add(wagonNode);return nodes;}

    private static void Replace<T>(ObservableCollection<T> target,IEnumerable<T> source){target.Clear();foreach(var item in source)target.Add(item);}
    private void SetStatus(string message,string color){StatusMessage=message;StatusBackground=new SolidColorBrush(Color.Parse(color));}
}
