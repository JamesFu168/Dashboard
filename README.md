# 小型部門看板系統 Dashboard

本專案是一個小型部門使用的看板系統規劃與原型，目標是建立類似 Trello 的任務管理體驗，並加入部門權限、Task 指派、即時同步與 Azure DevOps 工作項目連結。

目前專案以系統架構與開發規格整理為主，後續預計實作為：

- 前端：Angular 18+
- 後端：.NET 10 Web API
- 資料庫：Azure SQL Database
- ORM：Entity Framework Core
- 即時同步：SignalR

## 核心需求

- 提供 `Plan`、`To Do`、`Doing`、`Done` 四欄式 Kanban Board。
- 支援個人看板與部門/組織看板。
- Card Owner 可以建立、編輯、刪除、移動卡片。
- Task Assignee 可以看見被指派的卡片，但不能移動卡片。
- Task Assignee 可以勾選自己被指派的 Task 完成狀態。
- Card 與 Task 可連結 Azure DevOps PBI、Feature、Bug 或其他工作項目。
- 支援 Due Date、排序 `SequenceOrder`、即時更新與多人編輯衝突處理。

## 文件索引

- [系統架構規劃書](./kanban_system_architecture_plan.md)  
  包含系統架構圖、ERD、權限矩陣、API 設計、DTO 範例、錯誤處理與 SignalR 事件設計。

- [開發執行計畫](./implementation_plan.md)  
  包含後端、資料庫、前端、測試與部署的分階段開發 checklist。

## 主要資料模型

目前規劃的核心資料表包含：

- `USERS`
- `DEPARTMENTS`
- `CARDS`
- `CARD_TASKS`

`CARDS` 與 `CARD_TASKS` 都支援：

- `DueDate`：僅記錄日期，使用 `date` 型態。
- `SequenceOrder`：控制看板或 Task 列表排序。
- `DevOpsUrl`：連結 Azure DevOps 工作項目。
- `UpdatedAt`：支援樂觀鎖與多人編輯衝突判斷。

## API 規劃摘要

Card Endpoints：

```http
GET    /api/v1/cards?viewMode={personal|organization}
GET    /api/v1/cards/{id}
POST   /api/v1/cards
PATCH  /api/v1/cards/{id}
PUT    /api/v1/cards/{id}/status
DELETE /api/v1/cards/{id}
```

Task Endpoints：

```http
POST   /api/v1/cards/{cardId}/tasks
PATCH  /api/v1/tasks/{taskId}
PATCH  /api/v1/tasks/{taskId}/toggle
PUT    /api/v1/tasks/{taskId}/assign
DELETE /api/v1/tasks/{taskId}
```

## 權限原則

系統採用 Card Owner 與 Task Assignee 為核心的權限模型：

- Card Owner：可完整管理自己的卡片與 Task。
- Task Assignee：可檢視被指派 Task 所屬卡片，並勾選該 Task。
- 同部門成員：可檢視組織範圍卡片，但不可編輯或移動。
- 非相關人員：不可檢視個人卡片或其他部門卡片。

所有權限限制都需要同時在前端與後端實作：

- 前端：停用不可操作的按鈕、拖拉與 checkbox。
- 後端：以 API policy / domain service 回傳 `403 Forbidden`。

## 專案狀態

目前狀態：

- [x] 系統架構規劃
- [x] ERD 與資料欄位定義
- [x] 權限矩陣
- [x] API endpoint 初版
- [x] SignalR 事件規劃
- [ ] .NET 10 Web API 實作
- [ ] Angular 前端實作
- [ ] Azure SQL Database migration
- [ ] E2E 測試與部署

## 本地原型

目前目錄中包含一個簡易 UI POC：

```text
kanban-ui-poc/
```

此 POC 可作為後續 Angular 介面設計與互動流程的參考。

## 後續開發方向

建議下一步依照 [開發執行計畫](./implementation_plan.md) 進行：

1. 建立 .NET 10 Web API 專案骨架。
2. 建立 EF Core Entity、DbContext 與 Migration。
3. 實作 Card / Task API 與權限驗證。
4. 建立 Angular Kanban Board 與 Drag-and-Drop 操作。
5. 加入 SignalR 即時同步。
6. 補上 E2E 測試與部署流程。
