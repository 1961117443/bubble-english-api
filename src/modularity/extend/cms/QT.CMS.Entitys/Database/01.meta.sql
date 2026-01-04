/*
 Navicat Premium Data Transfer

 Source Server         : 117.21.203.8（centos数据库33100,root,95033##Cn123456）
 Source Server Type    : MySQL
 Source Server Version : 80036
 Source Host           : 117.21.203.8:33100
 Source Schema         : erp_tenant_cms2100_dev

 Target Server Type    : MySQL
 Target Server Version : 80036
 File Encoding         : 65001

 Date: 20/09/2024 10:48:10
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for cms_article_album
-- ----------------------------
DROP TABLE IF EXISTS `cms_article_album`;
CREATE TABLE `cms_article_album`  (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `ArticleId` bigint NOT NULL,
  `ThumbPath` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `OriginalPath` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Remark` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `SortId` int NOT NULL,
  `AddTime` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `IX_dt_article_album_ArticleId`(`ArticleId`) USING BTREE,
  CONSTRAINT `FK_dt_article_album_dt_articles_ArticleId` FOREIGN KEY (`ArticleId`) REFERENCES `cms_articles` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for cms_article_attach
-- ----------------------------
DROP TABLE IF EXISTS `cms_article_attach`;
CREATE TABLE `cms_article_attach`  (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `ArticleId` bigint NOT NULL,
  `FileName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `FilePath` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `FileSize` int NOT NULL,
  `FileExt` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Point` int NOT NULL,
  `DownCount` int NOT NULL,
  `SortId` int NOT NULL,
  `AddTime` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `IX_dt_article_attach_ArticleId`(`ArticleId`) USING BTREE,
  CONSTRAINT `FK_dt_article_attach_dt_articles_ArticleId` FOREIGN KEY (`ArticleId`) REFERENCES `cms_articles` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 2 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for cms_article_category
-- ----------------------------
DROP TABLE IF EXISTS `cms_article_category`;
CREATE TABLE `cms_article_category`  (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `SiteId` int NOT NULL,
  `ChannelId` int NOT NULL,
  `ParentId` bigint NOT NULL,
  `CallIndex` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Title` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `ImgUrl` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `LinkUrl` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Content` varchar(1024) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `SortId` int NOT NULL,
  `SeoTitle` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `SeoKeyword` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `SeoDescription` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Status` tinyint UNSIGNED NOT NULL,
  `AddBy` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `AddTime` datetime(6) NOT NULL,
  `UpdateBy` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `UpdateTime` datetime(6) NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 23 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for cms_article_category_relation
-- ----------------------------
DROP TABLE IF EXISTS `cms_article_category_relation`;
CREATE TABLE `cms_article_category_relation`  (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `ArticleId` bigint NOT NULL,
  `CategoryId` bigint NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `IX_dt_article_category_relation_ArticleId`(`ArticleId`) USING BTREE,
  INDEX `IX_dt_article_category_relation_CategoryId`(`CategoryId`) USING BTREE,
  CONSTRAINT `FK_dt_article_category_relation_dt_article_category_CategoryId` FOREIGN KEY (`CategoryId`) REFERENCES `cms_article_category` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT `FK_dt_article_category_relation_dt_articles_ArticleId` FOREIGN KEY (`ArticleId`) REFERENCES `cms_articles` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 82 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for cms_article_comment
-- ----------------------------
DROP TABLE IF EXISTS `cms_article_comment`;
CREATE TABLE `cms_article_comment`  (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `SiteId` int NOT NULL,
  `ChannelId` int NOT NULL,
  `ArticleId` bigint NOT NULL,
  `ParentId` bigint NOT NULL,
  `RootId` bigint NOT NULL,
  `UserId` int NOT NULL,
  `UserName` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `UserIp` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `AtUserId` int NOT NULL,
  `AtUserName` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Content` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `LikeCount` int NOT NULL,
  `Status` tinyint UNSIGNED NOT NULL,
  `IsDelete` tinyint UNSIGNED NOT NULL,
  `AddTime` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `IX_dt_article_comment_ArticleId`(`ArticleId`) USING BTREE,
  INDEX `IX_dt_article_comment_UserId`(`UserId`) USING BTREE,
  CONSTRAINT `FK_dt_article_comment_dt_articles_ArticleId` FOREIGN KEY (`ArticleId`) REFERENCES `cms_articles` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT `FK_dt_article_comment_dt_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `dt_users` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for cms_article_comment_like
-- ----------------------------
DROP TABLE IF EXISTS `cms_article_comment_like`;
CREATE TABLE `cms_article_comment_like`  (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `CommentId` bigint NOT NULL,
  `UserId` int NOT NULL,
  `AddTime` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `IX_dt_article_comment_like_CommentId`(`CommentId`) USING BTREE,
  CONSTRAINT `FK_dt_article_comment_like_dt_article_comment_CommentId` FOREIGN KEY (`CommentId`) REFERENCES `cms_article_comment` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for cms_article_contribute
-- ----------------------------
DROP TABLE IF EXISTS `cms_article_contribute`;
CREATE TABLE `cms_article_contribute`  (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `SiteId` int NOT NULL,
  `ChannelId` int NOT NULL,
  `Title` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Source` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Author` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `ImgUrl` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `FieldMeta` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `AlbumMeta` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `AttachMeta` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `Content` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `UserId` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `UserName` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Status` tinyint UNSIGNED NOT NULL,
  `AddTime` datetime(6) NOT NULL,
  `UpdateBy` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `UpdateTime` datetime(6) NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `IX_dt_article_contribute_ChannelId`(`ChannelId`) USING BTREE,
  CONSTRAINT `FK_dt_article_contribute_dt_site_channel_ChannelId` FOREIGN KEY (`ChannelId`) REFERENCES `cms_site_channel` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for cms_article_field_value
-- ----------------------------
DROP TABLE IF EXISTS `cms_article_field_value`;
CREATE TABLE `cms_article_field_value`  (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `ArticleId` bigint NOT NULL,
  `FieldId` bigint NOT NULL,
  `FieldName` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `FieldValue` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `IX_dt_article_field_value_ArticleId`(`ArticleId`) USING BTREE,
  CONSTRAINT `FK_dt_article_field_value_dt_articles_ArticleId` FOREIGN KEY (`ArticleId`) REFERENCES `cms_articles` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 20 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for cms_article_label
-- ----------------------------
DROP TABLE IF EXISTS `cms_article_label`;
CREATE TABLE `cms_article_label`  (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `Title` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `SortId` int NOT NULL,
  `Status` tinyint UNSIGNED NOT NULL,
  `AddBy` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `AddTime` datetime(6) NOT NULL,
  `UpdateBy` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `UpdateTime` datetime(6) NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 2 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for cms_article_label_relation
-- ----------------------------
DROP TABLE IF EXISTS `cms_article_label_relation`;
CREATE TABLE `cms_article_label_relation`  (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `ArticleId` bigint NOT NULL,
  `LabelId` bigint NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `IX_dt_article_label_relation_ArticleId`(`ArticleId`) USING BTREE,
  INDEX `IX_dt_article_label_relation_LabelId`(`LabelId`) USING BTREE,
  CONSTRAINT `FK_dt_article_label_relation_dt_article_label_LabelId` FOREIGN KEY (`LabelId`) REFERENCES `cms_article_label` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT `FK_dt_article_label_relation_dt_articles_ArticleId` FOREIGN KEY (`ArticleId`) REFERENCES `cms_articles` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for cms_articles
-- ----------------------------
DROP TABLE IF EXISTS `cms_articles`;
CREATE TABLE `cms_articles`  (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `SiteId` int NOT NULL,
  `ChannelId` int NOT NULL,
  `CallIndex` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Title` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Source` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Author` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `LinkUrl` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `ImgUrl` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `SeoTitle` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `SeoKeyword` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `SeoDescription` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Zhaiyao` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Content` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `SortId` int NOT NULL,
  `Click` int NOT NULL,
  `GroupIds` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Status` tinyint UNSIGNED NOT NULL,
  `IsComment` tinyint UNSIGNED NOT NULL,
  `CommentCount` int NOT NULL,
  `LikeCount` int NOT NULL,
  `AddBy` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `AddTime` datetime(6) NOT NULL,
  `UpdateBy` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `UpdateTime` datetime(6) NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `IX_dt_articles_ChannelId`(`ChannelId`) USING BTREE,
  CONSTRAINT `FK_dt_articles_dt_site_channel_ChannelId` FOREIGN KEY (`ChannelId`) REFERENCES `cms_site_channel` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 24 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for cms_contact
-- ----------------------------
DROP TABLE IF EXISTS `cms_contact`;
CREATE TABLE `cms_contact`  (
  `Id` int NOT NULL AUTO_INCREMENT COMMENT '主键',
  `Phone` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '联系电话',
  `Subject` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '主题',
  `IP` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'ip地址',
  `IsSend` tinyint UNSIGNED NOT NULL COMMENT '是否发送短信',
  `AddBy` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '提交人',
  `AddTime` datetime(6) NULL DEFAULT NULL COMMENT '提交时间',
  `SiteId` int NOT NULL COMMENT '站点',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 13 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for cms_manager
-- ----------------------------
DROP TABLE IF EXISTS `cms_manager`;
CREATE TABLE `cms_manager`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Avatar` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `RealName` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `IsAudit` tinyint UNSIGNED NOT NULL,
  `AddTime` datetime(6) NOT NULL,
  `LastIp` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `LastTime` datetime(6) NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  UNIQUE INDEX `IX_dt_manager_UserId`(`UserId`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for cms_manager_menu_model
-- ----------------------------
DROP TABLE IF EXISTS `cms_manager_menu_model`;
CREATE TABLE `cms_manager_menu_model`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ParentId` int NOT NULL,
  `NavType` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Name` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Title` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `SubTitle` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `IconUrl` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `LinkUrl` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Controller` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Resource` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `SortId` int NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 7 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for cms_member_group
-- ----------------------------
DROP TABLE IF EXISTS `cms_member_group`;
CREATE TABLE `cms_member_group`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Title` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `MinExp` int NOT NULL,
  `MaxExp` int NOT NULL,
  `Amount` decimal(18, 2) NOT NULL,
  `Discount` int NOT NULL,
  `IsUpgrade` tinyint UNSIGNED NOT NULL,
  `IsDefault` tinyint UNSIGNED NOT NULL,
  `AddBy` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `AddTime` datetime(6) NOT NULL,
  `UpdateBy` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `UpdateTime` datetime(6) NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 2 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for cms_members
-- ----------------------------
DROP TABLE IF EXISTS `cms_members`;
CREATE TABLE `cms_members`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `SiteId` int NOT NULL,
  `UserId` int NOT NULL,
  `GroupId` int NOT NULL,
  `Avatar` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `RealName` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Sex` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Birthday` datetime(6) NULL DEFAULT NULL,
  `Amount` decimal(18, 2) NOT NULL,
  `Point` int NOT NULL,
  `Exp` int NOT NULL,
  `RegIp` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `RegTime` datetime(6) NOT NULL,
  `LastIp` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `LastTime` datetime(6) NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  UNIQUE INDEX `IX_dt_members_UserId`(`UserId`) USING BTREE,
  INDEX `IX_dt_members_GroupId`(`GroupId`) USING BTREE,
  CONSTRAINT `FK_dt_members_dt_member_group_GroupId` FOREIGN KEY (`GroupId`) REFERENCES `cms_member_group` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT `FK_dt_members_dt_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `dt_users` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 2 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for cms_site_channel
-- ----------------------------
DROP TABLE IF EXISTS `cms_site_channel`;
CREATE TABLE `cms_site_channel`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `SiteId` int NOT NULL,
  `Name` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Title` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `IsComment` tinyint UNSIGNED NOT NULL,
  `IsAlbum` tinyint UNSIGNED NOT NULL,
  `IsAttach` tinyint UNSIGNED NOT NULL,
  `IsContribute` tinyint UNSIGNED NOT NULL,
  `SortId` int NOT NULL,
  `Status` tinyint UNSIGNED NOT NULL,
  `AddBy` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `AddTime` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `IX_dt_site_channel_SiteId`(`SiteId`) USING BTREE,
  CONSTRAINT `FK_dt_site_channel_dt_sites_SiteId` FOREIGN KEY (`SiteId`) REFERENCES `cms_sites` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 16 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for cms_site_channel_field
-- ----------------------------
DROP TABLE IF EXISTS `cms_site_channel_field`;
CREATE TABLE `cms_site_channel_field`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ChannelId` int NOT NULL,
  `Name` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Title` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ControlType` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ItemOption` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `DefaultValue` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `IsPassword` tinyint UNSIGNED NOT NULL,
  `IsRequired` tinyint UNSIGNED NOT NULL,
  `EditorType` tinyint UNSIGNED NOT NULL,
  `ValidTipMsg` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `ValidErrorMsg` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `ValidPattern` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `SortId` int NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `IX_dt_site_channel_field_ChannelId`(`ChannelId`) USING BTREE,
  CONSTRAINT `FK_dt_site_channel_field_dt_site_channel_ChannelId` FOREIGN KEY (`ChannelId`) REFERENCES `cms_site_channel` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 5 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for cms_site_domain
-- ----------------------------
DROP TABLE IF EXISTS `cms_site_domain`;
CREATE TABLE `cms_site_domain`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `SiteId` int NOT NULL,
  `Domain` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Remark` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `IX_dt_site_domain_SiteId`(`SiteId`) USING BTREE,
  CONSTRAINT `FK_dt_site_domain_dt_sites_SiteId` FOREIGN KEY (`SiteId`) REFERENCES `cms_sites` (`Id`) ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 23 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for cms_sites
-- ----------------------------
DROP TABLE IF EXISTS `cms_sites`;
CREATE TABLE `cms_sites`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `DirPath` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Title` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Logo` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Company` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Address` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Tel` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Fax` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Email` varchar(60) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Crod` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `Copyright` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `SeoTitle` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `SeoKeyword` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `SeoDescription` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `SortId` int NOT NULL,
  `IsDefault` tinyint UNSIGNED NOT NULL,
  `Status` tinyint UNSIGNED NOT NULL,
  `AddBy` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `AddTime` datetime(6) NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for cms_sysconfig
-- ----------------------------
DROP TABLE IF EXISTS `cms_sysconfig`;
CREATE TABLE `cms_sysconfig`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Type` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `JsonData` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 4 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

SET FOREIGN_KEY_CHECKS = 1;
