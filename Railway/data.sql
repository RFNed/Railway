INSERT INTO `employees` (`employee_id`, `last_name`, `first_name`, `middle_name`, `birth_date`, `phone`, `email`, `job_title`, `rating`) VALUES
(1, 'Иванов', 'Иван', 'Иванович', '1982-05-15', '+7 911 123 45 67', 'ivanov@railway.ru', 'manager', 100),
(2, 'Петров', 'Пётр', 'Сергеевич', '1978-11-20', '+7 921 234 56 78', 'petrov@railway.ru', 'driver', 75),
(3, 'Сидоров', 'Алексей', 'Михайлович', '1990-03-10', '+7 902 345 67 89', 'sidorov@railway.ru', 'assistant', 75),
(4, 'Смирнов', 'Дмитрий', 'Олегович', '1985-08-12', '+7 911 456 78 90', 'smirnov@railway.ru', 'driver', 25),
(5, 'Кузнецов', 'Максим', 'Игоревич', '1995-12-01', '+7 921 567 89 01', 'kuznetsov@railway.ru', 'assistant', 25);

INSERT INTO `locomotives` (`locomotive_id`, `locomotive_number`) VALUES
(1, 'ЛК349'),
(2, 'ЛК350'),
(3, 'ВЛ101');

INSERT INTO `wagons` (`wagon_id`, `wagon_number`, `wagon_type_id`) VALUES
(1, 'ОП10001', (SELECT `wagon_type_id` FROM `wagon_types` WHERE `wagon_code` = 'ОП')),
(2, 'ХП10002', (SELECT `wagon_type_id` FROM `wagon_types` WHERE `wagon_code` = 'ХП')),
(3, 'КВ10003', (SELECT `wagon_type_id` FROM `wagon_types` WHERE `wagon_code` = 'КВ')),
(4, 'ЦС10004', (SELECT `wagon_type_id` FROM `wagon_types` WHERE `wagon_code` = 'ЦС')),
(5, 'ЦС10005', (SELECT `wagon_type_id` FROM `wagon_types` WHERE `wagon_code` = 'ЦС')),
(6, 'ОП10006', (SELECT `wagon_type_id` FROM `wagon_types` WHERE `wagon_code` = 'ОП')),
(7, 'ДМ10007', (SELECT `wagon_type_id` FROM `wagon_types` WHERE `wagon_code` = 'ДМ'));

INSERT INTO `trains` (`train_id`, `train_number`, `departure_city_id`, `arrival_city_id`, `formation_datetime`, `manager_id`, `driver_id`, `assistant_id`) VALUES
(1, 'ПЗ-2026-01', 
  (SELECT `city_id` FROM `cities` WHERE `city_name` = 'Мурманск'), 
  (SELECT `city_id` FROM `cities` WHERE `city_name` = 'Апатиты'), 
  '2026-09-01 10:30:00', 1, 2, 3),
(2, 'ПЗ-2026-02', 
  (SELECT `city_id` FROM `cities` WHERE `city_name` = 'Кировск'), 
  (SELECT `city_id` FROM `cities` WHERE `city_name` = 'Вологда'), 
  '2026-09-02 14:15:00', 1, 4, 5);

INSERT INTO `train_locomotives` (`train_id`, `locomotive_id`) VALUES
(1, 1),
(1, 2),
(2, 3);

INSERT INTO `train_wagons` (`train_id`, `wagon_id`, `is_loaded`) VALUES
(1, 1, 1),
(1, 2, 0),
(1, 3, 1),
(1, 4, 1),
(1, 5, 0),
(1, 6, 1),
(1, 7, 1);
