# 小型部門看板系統開發執行計畫 (Implementation Plan)

本計畫為「小型部門看板系統」的開發執行計畫，包含後端 .NET 10 Web API、Azure SQL Database 以及前端 Angular 應用的建置與驗證流程。

---

## 階段一：後端基礎建設與資料庫 Entity 設計 (.NET 10 & EF Core)

- [x] **1.1 建立 .NET 10 Web API 專案骨架**
  - 初始化 ASP.NET Core Web API 專案 (`Dashboard.Api`)。
  - 配置 `appsettings.json` 與 connection string。
  - 導入 JWT Authentication（Access Token + Refresh Token 雙 Token 機制）與 .NET 10 原生 OpenAPI + Scalar UI。

- [x] **1.2 建立 EF Core Entities 與 DbContext**
  - 建立 `User`, `Department`, `Card`, `CardTask`, `RefreshToken` 實體。
  - `Card` 新增 `IsDeleted` 與 `DeletedAt` 軟刪除欄位，並配置全域 Query Filter (`HasQueryFilter(c => !c.IsDeleted)`)。
  - 設定 Enum：`CardStatus` (`Plan`, `ToDo`, `Doing`, `Done`, `AutoClosed`) 與 `CardScope` (`Personal`, `Organization`)。
  - 所有時間戳記欄位（`CreatedAt`/`UpdatedAt`/`DeletedAt`/Token 到期時間）統一透過 `DateTimeProvider.TaiwanNow` 取得台灣時間（UTC+8）。

- [x] **1.3 資料庫 Migration 與 Initial Seed Data**
  - 執行 EF Core Migration 建置資料庫 Schema。
  - 建立種子資料 (預設部門、測試使用者、初始範例卡片與 Tasks)。

---

## 階段二：後端核心商業邏輯與 API 開發

- [x] **2.1 Auth API 與密碼驗證**
  - 實作 `POST /api/v1/auth/login`、`POST /api/v1/auth/refresh`、`POST /api/v1/auth/logout`。

- [x] **2.2 Card API、軟刪除與每月自動結案機制**
  - 實作 `GET /api/v1/cards`（預設自動排除軟刪除 `IsDeleted = true` 與已結案 `AutoClosed` 之卡片）。
  - 實作 `POST /api/v1/cards`、`PATCH /api/v1/cards/{id}`、`PUT /api/v1/cards/{id}/status`。
  - 實作 `DELETE /api/v1/cards/{id}`（軟刪除移至回收桶，預設不顯示於看板）。
  - 實作 `POST /api/v1/cards/auto-close-previous-month`（批次將上個月已完成 `Done` 之卡片狀態自動更新為 `AutoClosed` 結案）。
  - 實作 `GET /api/v1/cards/closed`（取得歷史已結案卡片列表）。
  - 實作 `GET /api/v1/cards/trash`、`POST /api/v1/cards/{id}/restore`、`DELETE /api/v1/cards/{id}/permanent`（回收桶管理）。

- [x] **2.3 Task API 與人員指派限制**
  - 實作 `POST /api/v1/cards/{cardId}/tasks`、`PATCH /api/v1/tasks/{taskId}/toggle`、`PUT /api/v1/tasks/{taskId}/assign` (個人卡片限制禁止指派他人)、`DELETE /api/v1/tasks/{taskId}`。

- [x] **2.4 SignalR 即時溝通 Hub 建立**
  - 建立 `KanbanHub` 廣播卡片與 Task 即時異動。

- [x] **2.5 User 查詢 API**
  - 實作 `GET /api/v1/users?departmentId={id}`。

---

## 階段三：前端 Angular 看板系統開發

- [x] **3.1 專案建置與 UI 庫整合**
  - 建置 Angular 18+ 專案，導入 `@angular/cdk/drag-drop` 與 Angular Signals 狀態管理。

- [x] **3.2 主看板頁面與 4 個 Column 元件**
  - 建立 `KanbanBoardPageComponent` 展現 **Plan**, **To Do**, **Doing**, **Done** 四個 Column。已結案 (`AutoClosed`) 與已刪除 (`IsDeleted`) 之卡片預設不顯示在看板上。

- [x] **3.3 權限連動與 Drag-and-Drop 限制**
  - 卡片拖拉權限連動與 Task 勾選限制。

- [x] **3.4 卡片與 Task 管理對話框**
  - `CardFormDialogComponent` 與 `CreateTaskDialogComponent`。

- [x] **3.5 回收桶管理對話框**
  - `TrashDialogComponent`。

---

## 階段四：全系統整合、測試與部署驗證

- [x] **4.1 端到端整合測試 (E2E / Functional Test)**
  - 驗證自動結案機制：呼叫 `auto-close-previous-month` 後，前一個月 `Done` 的卡片自動變更為 `AutoClosed` 並隱藏於看板上。
  - 驗證已刪除與已結案之卡片預設一律不顯示於看板主介面。

- [x] **4.2 部署設定**
  - 維護 Azure SQL Database 腳本與 CI/CD YAML Pipelines。
