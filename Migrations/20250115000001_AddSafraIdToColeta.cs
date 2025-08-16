using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.coleta.Migrations
{
    /// <inheritdoc />
    public partial class AddSafraIdToColeta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if the column already exists and modify it if needed
            migrationBuilder.Sql(@"
                SET @column_exists = (
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'Coletas' 
                    AND COLUMN_NAME = 'SafraID'
                );
                
                SET @sql = IF(@column_exists = 0, 
                    'ALTER TABLE `Coletas` ADD `SafraID` char(36) COLLATE ascii_general_ci NULL',
                    'ALTER TABLE `Coletas` MODIFY `SafraID` char(36) COLLATE ascii_general_ci NULL'
                );
                
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Check if index exists before creating
            migrationBuilder.Sql(@"
                SET @index_exists = (
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.STATISTICS 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'Coletas' 
                    AND INDEX_NAME = 'IX_Coletas_SafraID'
                );
                
                SET @sql = IF(@index_exists = 0, 
                    'CREATE INDEX `IX_Coletas_SafraID` ON `Coletas` (`SafraID`)',
                    'SELECT ''Index already exists'' as message'
                );
                
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // First, clean up any invalid SafraID values
            migrationBuilder.Sql(@"
                UPDATE `Coletas` 
                SET `SafraID` = NULL 
                WHERE `SafraID` IS NOT NULL 
                AND `SafraID` NOT IN (SELECT `ID` FROM `Safras`);
            ");

            // Check if foreign key exists before creating
            migrationBuilder.Sql(@"
                SET @fk_exists = (
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'Coletas' 
                    AND COLUMN_NAME = 'SafraID'
                    AND REFERENCED_TABLE_NAME = 'Safras'
                );
            ");

            migrationBuilder.Sql(@"
                IF @fk_exists = 0 THEN
                    ALTER TABLE `Coletas` 
                    ADD CONSTRAINT `FK_Coletas_Safras_SafraID` 
                    FOREIGN KEY (`SafraID`) 
                    REFERENCES `Safras` (`ID`) 
                    ON DELETE SET NULL;
                END IF;
            ");

            migrationBuilder.Sql(@"
                IF @fk_exists > 0 THEN
                    SET @fk_name = (
                        SELECT CONSTRAINT_NAME 
                        FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
                        WHERE TABLE_SCHEMA = DATABASE() 
                        AND TABLE_NAME = 'Coletas' 
                        AND COLUMN_NAME = 'SafraID'
                        AND REFERENCED_TABLE_NAME = 'Safras'
                        LIMIT 1
                    );
                    
                    SET @sql = CONCAT('ALTER TABLE `Coletas` DROP FOREIGN KEY `', @fk_name, '`');
                    PREPARE stmt FROM @sql;
                    EXECUTE stmt;
                    DEALLOCATE PREPARE stmt;
                    
                    ALTER TABLE `Coletas` 
                    ADD CONSTRAINT `FK_Coletas_Safras_SafraID` 
                    FOREIGN KEY (`SafraID`) 
                    REFERENCES `Safras` (`ID`) 
                    ON DELETE SET NULL;
                END IF;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Coletas_Safras_SafraID",
                table: "Coletas");

            migrationBuilder.DropIndex(
                name: "IX_Coletas_SafraID",
                table: "Coletas");

            migrationBuilder.DropColumn(
                name: "SafraID",
                table: "Coletas");
        }
    }
}