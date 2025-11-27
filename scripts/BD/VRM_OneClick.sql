CREATE DATABASE  IF NOT EXISTS `vrm` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `vrm`;
-- MySQL dump 10.13  Distrib 8.0.43, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: vrm
-- ------------------------------------------------------
-- Server version	8.0.43

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `cat_actions`
--

DROP TABLE IF EXISTS `cat_actions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cat_actions` (
  `action_id` int NOT NULL AUTO_INCREMENT,
  `action_name` varchar(100) NOT NULL,
  `description` varchar(100) NOT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '0',
  `created_date` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_date` datetime DEFAULT NULL,
  `created_user_id` int NOT NULL,
  `updated_user_id` int DEFAULT NULL,
  PRIMARY KEY (`action_id`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `cat_actions`
--

LOCK TABLES `cat_actions` WRITE;
/*!40000 ALTER TABLE `cat_actions` DISABLE KEYS */;
INSERT INTO `cat_actions` VALUES (1,'Editar','Accion para editar campos',0,'2025-11-11 20:50:59',NULL,1,NULL),(2,'TimbrarSAT','Accion para timbrar al SAT',0,'2025-11-11 20:50:59',NULL,1,NULL),(3,'Eliminar','',0,'2025-11-11 20:50:59',NULL,1,NULL),(4,'Nueva','Nueva Factura',0,'2025-11-11 21:12:58',NULL,1,NULL);
/*!40000 ALTER TABLE `cat_actions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `cat_components`
--

DROP TABLE IF EXISTS `cat_components`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cat_components` (
  `component_id` int NOT NULL AUTO_INCREMENT,
  `component_name` varchar(100) NOT NULL,
  `module_id` int NOT NULL,
  `parent_id` int DEFAULT NULL,
  `description` varchar(500) NOT NULL,
  `route` varchar(300) NOT NULL,
  `icon` varchar(100) NOT NULL,
  `show_in_menu` tinyint(1) NOT NULL DEFAULT '0',
  `menu_order` int NOT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '0',
  `created_date` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_date` datetime DEFAULT NULL,
  `created_user_id` int NOT NULL,
  `updated_user_id` int DEFAULT NULL,
  PRIMARY KEY (`component_id`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `cat_components`
--

LOCK TABLES `cat_components` WRITE;
/*!40000 ALTER TABLE `cat_components` DISABLE KEYS */;
INSERT INTO `cat_components` VALUES (1,'Finanzas',1,NULL,'Módulo principal de finanzas','','ri-money-dollar-circle-line',1,20,1,'2025-11-11 20:24:42',NULL,1,NULL),(2,'Facturas',1,1,'Gestión de facturas','/finanzas/facturas','ri-file-list-3-line',1,1,1,'2025-11-11 20:24:42',NULL,1,NULL);
/*!40000 ALTER TABLE `cat_components` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `cat_modules`
--

DROP TABLE IF EXISTS `cat_modules`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cat_modules` (
  `module_id` int NOT NULL AUTO_INCREMENT,
  `module_name` varchar(100) NOT NULL,
  `display_name` varchar(100) NOT NULL,
  `description` varchar(300) NOT NULL,
  `version` varchar(100) NOT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '0',
  `created_date` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_date` datetime DEFAULT NULL,
  `created_user_id` int NOT NULL,
  `updated_user_id` int DEFAULT NULL,
  PRIMARY KEY (`module_id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `cat_modules`
--

LOCK TABLES `cat_modules` WRITE;
/*!40000 ALTER TABLE `cat_modules` DISABLE KEYS */;
INSERT INTO `cat_modules` VALUES (1,'Finanzas','Gestión de Finanzas','Módulo para gestionar operaciones financieras. Incluye facturas, pagos, conciliaciones y cuentas por pagar.','1.0.0',0,'2025-11-11 20:23:51','2025-11-11 20:23:51',1,1);
/*!40000 ALTER TABLE `cat_modules` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `cat_roles`
--

DROP TABLE IF EXISTS `cat_roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cat_roles` (
  `role_id` int NOT NULL AUTO_INCREMENT,
  `role_name` varchar(100) NOT NULL,
  `description` varchar(300) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '0',
  `created_date` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_date` datetime DEFAULT NULL,
  `created_user_id` int NOT NULL,
  `updated_user_id` int DEFAULT NULL,
  PRIMARY KEY (`role_id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `cat_roles`
--

LOCK TABLES `cat_roles` WRITE;
/*!40000 ALTER TABLE `cat_roles` DISABLE KEYS */;
INSERT INTO `cat_roles` VALUES (1,'Admin','Acceso completo del sistema',0,'2025-11-11 20:59:13',NULL,1,NULL);
/*!40000 ALTER TABLE `cat_roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `cat_users`
--

DROP TABLE IF EXISTS `cat_users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cat_users` (
  `user_id` varchar(100) NOT NULL DEFAULT (uuid()),
  `email` varchar(150) NOT NULL,
  `user_name` varchar(50) NOT NULL,
  `role_id` int NOT NULL,
  `name` varchar(150) DEFAULT NULL,
  `last_name` varchar(150) DEFAULT NULL,
  `second_last_name` varchar(150) DEFAULT NULL,
  `password` varchar(255) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '0',
  `created_date` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_date` datetime DEFAULT NULL,
  `created_user_id` int NOT NULL,
  `updated_user_id` int DEFAULT NULL,
  PRIMARY KEY (`role_id`,`email`,`user_name`),
  CONSTRAINT `cat_users_ibfk_1` FOREIGN KEY (`role_id`) REFERENCES `cat_roles` (`role_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `cat_users`
--

LOCK TABLES `cat_users` WRITE;
/*!40000 ALTER TABLE `cat_users` DISABLE KEYS */;
INSERT INTO `cat_users` VALUES ('4899b263-bf7b-11f0-b882-25512ae0e3cf','jesusadmin@vrm.com','JesusAdmin',1,'Jesus','Admin','Admin','rlsK5kLWWPPRZwVWNhlAr3CiqxpXuHXFFNKzZbhA+VDS3pk5X5nND2RM0lau46DV',0,'2025-11-11 21:54:26',NULL,1,NULL);
/*!40000 ALTER TABLE `cat_users` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `info_actions_key`
--

DROP TABLE IF EXISTS `info_actions_key`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `info_actions_key` (
  `action_key_id` bigint unsigned NOT NULL DEFAULT (uuid_short()),
  `module_id` int NOT NULL,
  `component_id` int NOT NULL,
  `action_id` int NOT NULL,
  `is_critical` tinyint(1) NOT NULL DEFAULT '0',
  `description` varchar(100) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '0',
  `created_date` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_date` datetime DEFAULT NULL,
  `created_user_id` int NOT NULL,
  `updated_user_id` int DEFAULT NULL,
  PRIMARY KEY (`module_id`,`component_id`,`action_id`),
  UNIQUE KEY `unique_action_key_id` (`action_key_id`),
  KEY `component_id` (`component_id`),
  KEY `action_id` (`action_id`),
  CONSTRAINT `info_actions_key_ibfk_1` FOREIGN KEY (`module_id`) REFERENCES `cat_modules` (`module_id`),
  CONSTRAINT `info_actions_key_ibfk_2` FOREIGN KEY (`component_id`) REFERENCES `cat_components` (`component_id`),
  CONSTRAINT `info_actions_key_ibfk_3` FOREIGN KEY (`action_id`) REFERENCES `cat_actions` (`action_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `info_actions_key`
--

LOCK TABLES `info_actions_key` WRITE;
/*!40000 ALTER TABLE `info_actions_key` DISABLE KEYS */;
INSERT INTO `info_actions_key` VALUES (101643171884171265,1,2,1,0,'Edicion Campos',1,'2025-11-19 00:12:11',NULL,1,NULL),(101643171884171266,1,2,2,0,'Timbra ante el SAT',1,'2025-11-19 00:12:11',NULL,1,NULL),(101643171884171267,1,2,3,0,'Elimina Factura',1,'2025-11-19 00:12:11',NULL,1,NULL),(101633185632223234,1,2,4,0,'Crear nueva factura',1,'2025-11-11 21:29:55',NULL,1,NULL);
/*!40000 ALTER TABLE `info_actions_key` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `info_actions_roles`
--

DROP TABLE IF EXISTS `info_actions_roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `info_actions_roles` (
  `action_key_id` bigint unsigned NOT NULL,
  `role_id` int NOT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '0',
  `created_date` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_date` datetime DEFAULT NULL,
  `created_user_id` int NOT NULL,
  `updated_user_id` int DEFAULT NULL,
  PRIMARY KEY (`action_key_id`,`role_id`),
  KEY `role_id` (`role_id`),
  CONSTRAINT `info_actions_roles_ibfk_1` FOREIGN KEY (`role_id`) REFERENCES `cat_roles` (`role_id`),
  CONSTRAINT `info_actions_roles_ibfk_2` FOREIGN KEY (`action_key_id`) REFERENCES `info_actions_key` (`action_key_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `info_actions_roles`
--

LOCK TABLES `info_actions_roles` WRITE;
/*!40000 ALTER TABLE `info_actions_roles` DISABLE KEYS */;
INSERT INTO `info_actions_roles` VALUES (101643171884171265,1,1,'2025-11-11 21:32:11',NULL,1,NULL),(101643171884171266,1,1,'2025-11-18 23:47:20',NULL,1,NULL),(101643171884171267,1,1,'2025-11-19 00:13:08',NULL,1,NULL),(101633185632223234,1,1,'2025-11-19 00:13:08',NULL,1,NULL);
/*!40000 ALTER TABLE `info_actions_roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `info_component_roles`
--

DROP TABLE IF EXISTS `info_component_roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `info_component_roles` (
  `component_id` int NOT NULL,
  `role_id` int NOT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '0',
  `created_date` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_date` datetime DEFAULT NULL,
  `created_user_id` int NOT NULL,
  `updated_user_id` int DEFAULT NULL,
  PRIMARY KEY (`component_id`,`role_id`),
  KEY `role_id` (`role_id`),
  CONSTRAINT `info_component_roles_ibfk_1` FOREIGN KEY (`role_id`) REFERENCES `cat_roles` (`role_id`),
  CONSTRAINT `info_component_roles_ibfk_2` FOREIGN KEY (`component_id`) REFERENCES `cat_components` (`component_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `info_component_roles`
--

LOCK TABLES `info_component_roles` WRITE;
/*!40000 ALTER TABLE `info_component_roles` DISABLE KEYS */;
INSERT INTO `info_component_roles` VALUES (1,1,0,'2025-11-13 01:05:39',NULL,1,NULL),(2,1,0,'2025-11-13 01:05:39',NULL,1,NULL);
/*!40000 ALTER TABLE `info_component_roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping routines for database 'vrm'
--
/*!50003 DROP PROCEDURE IF EXISTS `sp_get_actions` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_get_actions`(
    IN module_id INT
)
BEGIN
    SELECT 
        a.action_key_id AS ActionKeyId,
        CONCAT(c.module_name, '.', d.component_name, '.', e.action_name) AS ActionKey,
        GROUP_CONCAT(b.role_id ORDER BY b.role_id SEPARATOR ',') AS Roles,
        b.is_active AS IsActive
    FROM Info_actions_key a
    INNER JOIN info_actions_roles b ON a.action_key_id = b.action_key_id
    INNER JOIN cat_modules c        ON a.module_id = c.module_id
    INNER JOIN cat_components d     ON a.component_id = d.component_id
    INNER JOIN cat_actions e        ON a.action_id = e.action_id
    WHERE a.module_id = module_id AND b.is_active = 1
    GROUP BY 
        a.action_key_id,
        c.module_name,
        d.component_name,
        e.action_name,
        b.is_active;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_get_component` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_get_component`(
    IN module_id INT
)
SELECT
    c.component_id,
    c.component_name as name,
    c.module_id,
    c.parent_id,
    c.description,
    c.route,
    c.icon,
    c.show_in_menu,
    c.menu_order,
    c.is_active,
    COALESCE(r.roles, '') AS roles
  FROM cat_components c
  LEFT JOIN (
    SELECT
      component_id,
      GROUP_CONCAT(DISTINCT role_id ORDER BY role_id SEPARATOR ',') AS roles
    FROM info_component_roles
    GROUP BY component_id
  ) r ON c.component_id = r.component_id
  WHERE c.module_id = module_id ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_get_module_info` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_get_module_info`(
    IN module_id INT
)
SELECT module_name,display_name,description, version
 FROM cat_modules a
  WHERE a.module_id = module_id ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_get_user` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_get_user`(
    IN login VARCHAR(30) 
)
BEGIN
    IF  login LIKE '%@%' THEN
		SELECT 
        user_id as userId,
		user_name as UserName,
		CONCAT(name,' ',last_name,' ',second_last_name) as NombreCompleto, 
		email , 
		b.role_name as role,
		a.role_id as RoleId , 
		password
			FROM cat_users a inner join cat_roles b on a.role_id = b.role_id
			WHERE a.Email = login; 
    ELSE
		SELECT 
        user_id,
        user_name,
		CONCAT(name,' ',last_name,' ',second_last_name) as NombreCompleto, 
		email ,
		b.role_name as role,
		a.role_id as RoleId  , 
		password
			FROM cat_users a inner join cat_roles b on a.role_id = b.role_id
			WHERE a.user_name = login;
    END IF;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_set_actionkey` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_set_actionkey`(
    IN module_id INT,
    IN component_id INT,
    IN action_id INT,
    IN description VARCHAR(100),
    IN created_user_id INT
)
BEGIN
INSERT INTO info_actions_key (module_id, component_id, action_id,description,created_user_id) values
(module_id, component_id, action_id, description,created_user_id);
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_set_actions` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_set_actions`(
    IN action_name VARCHAR(100),
    IN description VARCHAR(100),
    IN created_user_id INT
)
BEGIN
INSERT INTO cat_actions (action_name, description, created_user_id) values
(action_name, description, created_user_id);
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_set_action_role` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_set_action_role`(
    IN action_key_id BIGINT,
    IN role_id INT,
    IN created_user_id INT
)
BEGIN
INSERT INTO info_actions_roles (action_key_id, role_id, created_user_id) values
(action_key_id, role_id, created_user_id);
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_set_components` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_set_components`(
    IN component_name VARCHAR(100),
    IN module_id INT,
    IN parent_id INT,
    IN description VARCHAR (500),
    IN route VARCHAR (300),
    IN icon VARCHAR (100),
    IN show_in_menu tinyint(1),
    IN menu_order INT,
    in created_user_id INT
)
BEGIN
INSERT INTO cat_components (component_name, module_id,parent_id,description,route,icon,show_in_menu,menu_order,created_user_id) values
(component_name, module_id, parent_id,description,route,icon,show_in_menu,menu_order,created_user_id);
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_set_component_roles` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_set_component_roles`(
    IN component_id INT,
    IN role_id INT,
    IN created_user_id INT
)
BEGIN
INSERT INTO info_component_roles (component_id, role_id,created_user_id) VALUES
(component_id,role_id,created_user_id);
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_set_module` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_set_module`(
    IN module_name VARCHAR(100),
    IN display_name VARCHAR(100),
    IN description VARCHAR(300),
    IN version VARCHAR(100),
    IN created_user_id INT
)
BEGIN
INSERT INTO cat_modules (module_name, display_name,description,version, created_user_id) values
(module_name, display_name,description,version, created_user_id);
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_set_roles` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_set_roles`(
    IN role_name VARCHAR(150),
    IN description VARCHAR(150),
    IN crater_user INT
)
BEGIN
INSERT INTO cat_roles (role_name, description,created_user_id) values
(role_name, description, crater_user);
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_set_user` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_set_user`(
    IN email_user VARCHAR(150),
    IN username VARCHAR(150),
    IN role_user_id INT,
    IN name_user VARCHAR(150),
    IN last_name_user VARCHAR(150),
    IN second_last_name_user VARCHAR(150),
    IN password_user VARCHAR (255),
    IN crater_user INT
)
BEGIN
INSERT INTO cat_users (email, user_name, role_id,name,last_name,second_last_name,password,created_user_id) values
(email_user, username, role_user_id, name_user,last_name_user, second_last_name_user, password_user, crater_user);
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-11-27 11:05:17
