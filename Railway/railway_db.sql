-- phpMyAdmin SQL Dump
-- version 5.2.3
-- https://www.phpmyadmin.net/
--
-- Хост: MySQL-8.0:3306
-- Время создания: Сен 02 2026 г., 17:41
-- Версия сервера: 8.0.44
-- Версия PHP: 8.0.30

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";
CREATE DATABASE IF NOT EXISTS `railway_db` CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;
USE `railway_db`;


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- База данных: `railway_db`
--

-- --------------------------------------------------------

--
-- Структура таблицы `cities`
--

CREATE TABLE `cities` (
  `city_id` int NOT NULL,
  `city_name` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Структура таблицы `employees`
--

CREATE TABLE `employees` (
  `employee_id` int NOT NULL,
  `last_name` varchar(100) NOT NULL,
  `first_name` varchar(100) NOT NULL,
  `middle_name` varchar(100) NOT NULL,
  `birth_date` date NOT NULL,
  `phone` varchar(20) NOT NULL,
  `email` varchar(100) NOT NULL,
  `job_title` enum('manager','driver','assistant') CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `rating` int NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Структура таблицы `locomotives`
--

CREATE TABLE `locomotives` (
  `locomotive_id` int NOT NULL,
  `locomotive_number` char(5) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Структура таблицы `trains`
--

CREATE TABLE `trains` (
  `train_id` int NOT NULL,
  `train_number` varchar(20) NOT NULL,
  `departure_city_id` int NOT NULL,
  `arrival_city_id` int NOT NULL,
  `formation_datetime` datetime NOT NULL,
  `manager_id` int NOT NULL,
  `driver_id` int NOT NULL,
  `assistant_id` int NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Структура таблицы `train_locomotives`
--

CREATE TABLE `train_locomotives` (
  `train_locomotive_id` int NOT NULL,
  `train_id` int NOT NULL,
  `locomotive_id` int NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Структура таблицы `train_wagons`
--

CREATE TABLE `train_wagons` (
  `train_wagon_id` int NOT NULL,
  `train_id` int NOT NULL,
  `wagon_id` int NOT NULL,
  `is_loaded` tinyint(1) NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Структура таблицы `wagons`
--

CREATE TABLE `wagons` (
  `wagon_id` int NOT NULL,
  `wagon_number` char(7) NOT NULL,
  `wagon_type_id` int NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Структура таблицы `wagon_types`
--

CREATE TABLE `wagon_types` (
  `wagon_type_id` int NOT NULL,
  `wagon_code` char(2) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `description` varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


--
-- Индексы сохранённых таблиц
--

--
-- Индексы таблицы `cities`
--
ALTER TABLE `cities`
  ADD PRIMARY KEY (`city_id`);

--
-- Индексы таблицы `employees`
--
ALTER TABLE `employees`
  ADD PRIMARY KEY (`employee_id`);

--
-- Индексы таблицы `locomotives`
--
ALTER TABLE `locomotives`
  ADD PRIMARY KEY (`locomotive_id`),
  ADD UNIQUE KEY `uq_locomotive_number` (`locomotive_number`);

--
-- Индексы таблицы `trains`
--
ALTER TABLE `trains`
  ADD PRIMARY KEY (`train_id`),
  ADD UNIQUE KEY `train_number` (`train_number`),
  ADD KEY `manager_id` (`manager_id`),
  ADD KEY `driver_id` (`driver_id`),
  ADD KEY `assistant_id` (`assistant_id`),
  ADD KEY `departure_city_id` (`departure_city_id`),
  ADD KEY `arrival_city_id` (`arrival_city_id`);

--
-- Индексы таблицы `train_locomotives`
--
ALTER TABLE `train_locomotives`
  ADD PRIMARY KEY (`train_locomotive_id`),
  ADD UNIQUE KEY `uq_train_locomotive` (`locomotive_id`),
  ADD KEY `train_id` (`train_id`);

--
-- Индексы таблицы `train_wagons`
--
ALTER TABLE `train_wagons`
  ADD PRIMARY KEY (`train_wagon_id`),
  ADD UNIQUE KEY `uq_train_wagon` (`wagon_id`),
  ADD KEY `train_id` (`train_id`);

--
-- Индексы таблицы `wagons`
--
ALTER TABLE `wagons`
  ADD PRIMARY KEY (`wagon_id`),
  ADD UNIQUE KEY `uq_wagon_number` (`wagon_number`),
  ADD KEY `wagon_type_id` (`wagon_type_id`);

--
-- Индексы таблицы `wagon_types`
--
ALTER TABLE `wagon_types`
  ADD PRIMARY KEY (`wagon_type_id`),
  ADD UNIQUE KEY `uq_wagon_code` (`wagon_code`);

--
-- AUTO_INCREMENT для сохранённых таблиц
--

--
-- AUTO_INCREMENT для таблицы `cities`
--
ALTER TABLE `cities`
  MODIFY `city_id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT для таблицы `employees`
--
ALTER TABLE `employees`
  MODIFY `employee_id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT для таблицы `locomotives`
--
ALTER TABLE `locomotives`
  MODIFY `locomotive_id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT для таблицы `trains`
--
ALTER TABLE `trains`
  MODIFY `train_id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT для таблицы `train_locomotives`
--
ALTER TABLE `train_locomotives`
  MODIFY `train_locomotive_id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT для таблицы `train_wagons`
--
ALTER TABLE `train_wagons`
  MODIFY `train_wagon_id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT для таблицы `wagons`
--
ALTER TABLE `wagons`
  MODIFY `wagon_id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT для таблицы `wagon_types`
--
ALTER TABLE `wagon_types`
  MODIFY `wagon_type_id` int NOT NULL AUTO_INCREMENT;

-- Предопределённые пункты назначения и типы вагонов по ТЗ.
INSERT INTO `cities` (`city_name`)
SELECT 'Мурманск' WHERE NOT EXISTS (SELECT 1 FROM `cities` WHERE `city_name`='Мурманск');
INSERT INTO `cities` (`city_name`)
SELECT 'Кировск' WHERE NOT EXISTS (SELECT 1 FROM `cities` WHERE `city_name`='Кировск');
INSERT INTO `cities` (`city_name`)
SELECT 'Апатиты' WHERE NOT EXISTS (SELECT 1 FROM `cities` WHERE `city_name`='Апатиты');
INSERT INTO `cities` (`city_name`)
SELECT 'Вологда' WHERE NOT EXISTS (SELECT 1 FROM `cities` WHERE `city_name`='Вологда');
INSERT INTO `cities` (`city_name`)
SELECT 'Череповецк' WHERE NOT EXISTS (SELECT 1 FROM `cities` WHERE `city_name`='Череповецк');
INSERT INTO `wagon_types` (`wagon_code`,`description`)
SELECT 'ОП','Открытая платформа' WHERE NOT EXISTS (SELECT 1 FROM `wagon_types` WHERE `wagon_code`='ОП');
INSERT INTO `wagon_types` (`wagon_code`,`description`)
SELECT 'ДМ','Думпкар для перевозки щебня' WHERE NOT EXISTS (SELECT 1 FROM `wagon_types` WHERE `wagon_code`='ДМ');
INSERT INTO `wagon_types` (`wagon_code`,`description`)
SELECT 'КВ','Крытый вагон' WHERE NOT EXISTS (SELECT 1 FROM `wagon_types` WHERE `wagon_code`='КВ');
INSERT INTO `wagon_types` (`wagon_code`,`description`)
SELECT 'ЦС','Цистерна' WHERE NOT EXISTS (SELECT 1 FROM `wagon_types` WHERE `wagon_code`='ЦС');
INSERT INTO `wagon_types` (`wagon_code`,`description`)
SELECT 'ХП','Хоппер для сыпучих грузов' WHERE NOT EXISTS (SELECT 1 FROM `wagon_types` WHERE `wagon_code`='ХП');
INSERT INTO `wagon_types` (`wagon_code`,`description`)
SELECT 'ИЗ','Изотермический вагон' WHERE NOT EXISTS (SELECT 1 FROM `wagon_types` WHERE `wagon_code`='ИЗ');



ALTER TABLE `employees`
  ADD CONSTRAINT `chk_employee_birth_date` CHECK (YEAR(`birth_date`) BETWEEN 1960 AND 2005),
  ADD CONSTRAINT `chk_employee_phone` CHECK (`phone` REGEXP '^\\+7 [0-9]{3} [0-9]{3} [0-9]{2} [0-9]{2}$'),
  ADD CONSTRAINT `chk_employee_rating` CHECK (`rating` >= 0);

ALTER TABLE `locomotives`
  ADD CONSTRAINT `chk_locomotive_number` CHECK (`locomotive_number` REGEXP '^[А-ЯЁа-яёA-Za-z]{2}[0-9]{3}$');

ALTER TABLE `wagons`
  ADD CONSTRAINT `chk_wagon_number` CHECK (`wagon_number` REGEXP '^[А-ЯЁа-яёA-Za-z]{2}[0-9]{5}$');

--
-- Ограничения внешнего ключа сохраненных таблиц
--

--
-- Ограничения внешнего ключа таблицы `trains`
--
ALTER TABLE `trains`
  ADD CONSTRAINT `idx_t_c_ac` FOREIGN KEY (`arrival_city_id`) REFERENCES `cities` (`city_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  ADD CONSTRAINT `idx_t_c_dc` FOREIGN KEY (`departure_city_id`) REFERENCES `cities` (`city_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  ADD CONSTRAINT `idx_t_e_a` FOREIGN KEY (`assistant_id`) REFERENCES `employees` (`employee_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  ADD CONSTRAINT `idx_t_e_d` FOREIGN KEY (`driver_id`) REFERENCES `employees` (`employee_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  ADD CONSTRAINT `idx_t_e_m` FOREIGN KEY (`manager_id`) REFERENCES `employees` (`employee_id`) ON DELETE RESTRICT ON UPDATE CASCADE;

--
-- Ограничения внешнего ключа таблицы `train_locomotives`
--
ALTER TABLE `train_locomotives`
  ADD CONSTRAINT `idx_tl_l` FOREIGN KEY (`locomotive_id`) REFERENCES `locomotives` (`locomotive_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  ADD CONSTRAINT `idx_tl_t` FOREIGN KEY (`train_id`) REFERENCES `trains` (`train_id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Ограничения внешнего ключа таблицы `train_wagons`
--
ALTER TABLE `train_wagons`
  ADD CONSTRAINT `idx_tw_t` FOREIGN KEY (`train_id`) REFERENCES `trains` (`train_id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `idx_tw_w` FOREIGN KEY (`wagon_id`) REFERENCES `wagons` (`wagon_id`) ON DELETE RESTRICT ON UPDATE CASCADE;

--
-- Ограничения внешнего ключа таблицы `wagons`
--
ALTER TABLE `wagons`
  ADD CONSTRAINT `wagons_ibfk_1` FOREIGN KEY (`wagon_type_id`) REFERENCES `wagon_types` (`wagon_type_id`) ON DELETE RESTRICT ON UPDATE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
