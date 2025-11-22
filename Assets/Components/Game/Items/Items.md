# Items コンポーネント ドキュメント

Itemsコンポーネントは、ゲーム内のアイテム生成と管理を行うための機能群です。

## 概要

`ItemGenerator` スクリプトを中心に、`StageDatabase` から読み込んだデータに基づいて、ステージごとに必要なアイテムオブジェクトを生成・配置します。各アイテムはIDを持ち、`ItemAssigner` (および `ItemDatabase`) を通じて具体的なアイテムデータ（スプライトやパラメータ）が割り当てられます。

## ディレクトリ構成

```
Assets/Components/Game/Items/
├── Scripts/
│   ├── ItemGenerator.cs    # アイテム生成のメインロジック
│   └── ItemAssigner.cs     # (想定) 個別アイテムへのデータ割り当て
├── Prefab/                 # アイテムのプレハブ
├── Images/                 # アイテム画像など
└── Items.md                # 本ドキュメント
```

## 主なクラス

### 1. ItemGenerator (`Assets/Components/Game/Items/Scripts/ItemGenerator.cs`)

アイテムの生成と配置を担当する `MonoBehaviour` です。

#### 機能
*   **データ読み込み**: `StageDatabase` (ScriptableObject) から、指定されたステージインデックスのアイテムIDリスト (`itemIds`) を読み込みます。
*   **アイテム生成**: 読み込んだIDの数だけ `itemPrefab` をインスタンス化します。
*   **配置**: 生成したアイテムを横一列（X軸方向）に均等配置します。全体が中心に揃うようにオフセットが計算されます。
*   **初期化**: 生成したアイテムオブジェクトに対し、`ItemAssigner` コンポーネント経由でアイテムIDを割り当てます (`AssignItem`)。

#### インスペクター設定項目
*   **Settings**
    *   `Spacing X`: アイテム間の横方向の間隔。
    *   `Item Prefab`: 生成するアイテムのプレハブ。`ItemAssigner` コンポーネントがアタッチされている必要があります。
    *   `Item Parent`: 生成されたアイテムの親となるTransform。
*   **Data**
    *   `Stage Database`: ステージデータが格納された `StageDatabase` アセット。
    *   `Current Stage Index`: 読み込むステージのインデックス。

### 2. ItemAssigner (想定)

個々のアイテムオブジェクトにアタッチされ、IDに基づいてアイテムの外見やパラメータを設定するコンポーネントです。
（`ItemGenerator` から `AssignItem(string itemId)` が呼び出されます）

## 使い方

1.  `StageDatabase` アセットに、各ステージで使用するアイテムIDのリストを設定します。
2.  シーンに空のGameObjectを作成し、`ItemGenerator` コンポーネントをアタッチします。
3.  `ItemGenerator` のインスペクターで、`Item Prefab`, `Item Parent`, `Spacing X` を設定します。
4.  `Stage Database` フィールドにデータベースアセットを割り当て、`Current Stage Index` を指定します。
5.  ゲーム実行時（`Start` メソッド）に自動的にアイテムが生成・配置されます。
