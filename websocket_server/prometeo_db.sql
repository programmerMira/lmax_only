CREATE DATABASE  IF NOT EXISTS `prometeo_db` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `prometeo_db`;
-- MySQL dump 10.13  Distrib 8.0.18, for Win64 (x86_64)
--
-- Host: localhost    Database: prometeo_db
-- ------------------------------------------------------
-- Server version	8.0.18

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
-- Table structure for table `header_tokens`
--

DROP TABLE IF EXISTS `header_tokens`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `header_tokens` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `token` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `token_UNIQUE` (`token`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `header_tokens`
--

LOCK TABLES `header_tokens` WRITE;
/*!40000 ALTER TABLE `header_tokens` DISABLE KEYS */;
INSERT INTO `header_tokens` VALUES (1,'eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ');
/*!40000 ALTER TABLE `header_tokens` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `token_histories`
--

DROP TABLE IF EXISTS `token_histories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `token_histories` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user_to_token_id` int(11) NOT NULL,
  `action` varchar(50) NOT NULL,
  `created_at` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `token_histories_token_idx` (`user_to_token_id`),
  CONSTRAINT `token_histories_token` FOREIGN KEY (`user_to_token_id`) REFERENCES `user_to_tokens` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=20 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `token_histories`
--

LOCK TABLES `token_histories` WRITE;
/*!40000 ALTER TABLE `token_histories` DISABLE KEYS */;
INSERT INTO `token_histories` VALUES (1,1,'test1','2022-01-15 22:08:10'),(2,2,'test2','2022-01-15 22:08:10'),(3,1,'test3','2022-01-15 22:08:10'),(4,4,'Creation','2022-01-25 20:26:10'),(5,5,'Creation','2022-01-25 20:31:59'),(6,6,'Creation','2022-01-25 20:33:44'),(7,2,'Update status','2022-01-25 21:00:12'),(8,3,'Update status','2022-01-25 21:00:12'),(9,2,'Update status','2022-01-25 21:00:36'),(10,3,'Update status','2022-01-25 21:00:36'),(11,2,'Update status: True','2022-01-25 21:01:29'),(12,3,'Update status: True','2022-01-25 21:01:29'),(13,1,'Update status: True','2022-01-25 21:03:15'),(14,2,'Update status: False','2022-01-25 21:03:41'),(15,3,'Update status: False','2022-01-25 21:03:41'),(16,1,'Update status: False','2022-01-25 21:03:41'),(17,2,'Update status: True','2022-01-25 21:03:57'),(18,3,'Update status: True','2022-01-25 21:03:57'),(19,1,'Update status: True','2022-01-25 21:03:57');
/*!40000 ALTER TABLE `token_histories` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user_to_tokens`
--

DROP TABLE IF EXISTS `user_to_tokens`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_to_tokens` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `outer_id` varchar(50) DEFAULT NULL,
  `access_token` varchar(50) NOT NULL,
  `is_connected` tinyint(4) DEFAULT NULL,
  `created_at` datetime DEFAULT NULL,
  `updated_at` datetime DEFAULT NULL,
  `deleted_at` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_to_tokens`
--

LOCK TABLES `user_to_tokens` WRITE;
/*!40000 ALTER TABLE `user_to_tokens` DISABLE KEYS */;
INSERT INTO `user_to_tokens` VALUES (1,'123','qwertyuiopasdfghjkl',0,'2022-01-15 22:08:10',NULL,NULL),(2,'456','asdfghjxcvbnfghj',1,'2022-01-15 22:08:10','2022-01-15 22:08:10',NULL),(3,'456','cvvvvbvnbvy',0,'2022-01-15 22:08:10',NULL,NULL),(4,'text','f182a2811f19902d17aec51d456cebe4',0,'2022-01-25 20:26:10',NULL,'2022-01-25 20:26:10'),(5,NULL,'8fd978b229fd6faaf8eae87c56b91da0',0,'2022-01-25 20:31:59',NULL,'2022-01-25 20:31:59'),(6,NULL,'69d2ac76186639eaaf3993a8f134b56f',0,'2022-01-25 20:33:44',NULL,NULL);
/*!40000 ALTER TABLE `user_to_tokens` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping events for database 'prometeo_db'
--

--
-- Dumping routines for database 'prometeo_db'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2022-01-25 22:47:59
