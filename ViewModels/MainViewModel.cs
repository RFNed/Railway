using System;
using System.IO;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RailwayApp.Models;
using RailwayApp.Services;

namespace RailwayApp.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;

    [ObservableProperty]
    private ObservableCollection<Train> _trains = [];

    [ObservableProperty]
    private Train? _selectedTrain;

    partial void OnSelectedTrainChanged(Train? value)
    {
        LoadEditData();
    }

    public ObservableCollection<LookupItem> Cities { get; } = [];
    public ObservableCollection<LookupItem> Managers { get; } = [];
    public ObservableCollection<LookupItem> Drivers { get; } = [];
    public ObservableCollection<LookupItem> Assistants { get; } = [];

    [ObservableProperty]
    private string _editNumber = string.Empty;

    [ObservableProperty]
    private LookupItem? _selectedDepCity;

    [ObservableProperty]
    private LookupItem? _selectedArrCity;

    [ObservableProperty]
    private LookupItem? _selectedManager;

    [ObservableProperty]
    private LookupItem? _selectedDriver;

    [ObservableProperty]
    private LookupItem? _selectedAssistant;

    [ObservableProperty]
    private string _statusMessage = "Готово";

    public MainWindowViewModel()
    {
        IConfiguration config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
        string connectionString = config.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Строка подключения 'DefaultConnection' не найдена в appsettings.json");
        
        _dbService = new DatabaseService(connectionString);

        _ = LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            StatusMessage = "Загрузка...";

            var cities = await _dbService.GetCitiesAsync();
            Cities.Clear();
            foreach (var c in cities) Cities.Add(c);

            var managers = await _dbService.GetEmployeesByRoleAsync("manager");
            Managers.Clear();
            foreach (var m in managers) Managers.Add(m);

            var drivers = await _dbService.GetEmployeesByRoleAsync("driver");
            Drivers.Clear();
            foreach (var d in drivers) Drivers.Add(d);

            var assistants = await _dbService.GetEmployeesByRoleAsync("assistant");
            Assistants.Clear();
            foreach (var a in assistants) Assistants.Add(a);

            var trains = await _dbService.GetTrainsAsync();
            Trains.Clear();
            foreach (var t in trains) Trains.Add(t);

            StatusMessage = "Данные успешно загружены";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка подключения/БД: {ex.Message}";
        }
    }

    private void LoadEditData()
    {
        if (SelectedTrain == null) return;

        EditNumber = SelectedTrain.TrainNumber;
        SelectedDepCity = Cities.FirstOrDefault(x => x.Id == SelectedTrain.DepartureCityId);
        SelectedArrCity = Cities.FirstOrDefault(x => x.Id == SelectedTrain.ArrivalCityId);
        SelectedManager = Managers.FirstOrDefault(x => x.Id == SelectedTrain.ManagerId);
        SelectedDriver = Drivers.FirstOrDefault(x => x.Id == SelectedTrain.DriverId);
        SelectedAssistant = Assistants.FirstOrDefault(x => x.Id == SelectedTrain.AssistantId);
    }

    [RelayCommand]
    private async Task SaveTrainAsync()
    {
        if (SelectedTrain == null)
        {
            StatusMessage = "Выберите состав в таблице для редактирования!";
            return;
        }

        if (SelectedDepCity == null || SelectedArrCity == null ||
            SelectedManager == null || SelectedDriver == null || SelectedAssistant == null)
        {
            StatusMessage = "Заполните все выпадающие поля!";
            return;
        }

        try
        {
            SelectedTrain.TrainNumber = EditNumber;
            SelectedTrain.DepartureCityId = SelectedDepCity.Id;
            SelectedTrain.ArrivalCityId = SelectedArrCity.Id;
            SelectedTrain.ManagerId = SelectedManager.Id;
            SelectedTrain.DriverId = SelectedDriver.Id;
            SelectedTrain.AssistantId = SelectedAssistant.Id;

            bool updated = await _dbService.UpdateTrainAsync(SelectedTrain);
            if (updated)
            {
                StatusMessage = "Состав сохранен!";
                await LoadDataAsync();
            }
            else
            {
                StatusMessage = "Не удалось обновить запись";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка сохранения: {ex.Message}";
        }
    }
}