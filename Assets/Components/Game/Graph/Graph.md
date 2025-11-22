# Graph コンポーネント ドキュメント

Graphコンポーネントは、ゲーム内のタスクグラフ（ノードとエッジ）を生成・管理するための機能群です。

## 概要

`GraphGenerator` スクリプトを中心に、`StageDatabase` から読み込んだデータに基づいて、3D空間（または2D平面）上にグラフ構造を可視化します。各ノード（頂点）はタスクを表し、エッジ（辺）はタスク間の依存関係を表します。

## ディレクトリ構成

```
Assets/Components/Game/Graph/
├── Scripts/
│   └── GraphGenerator.cs    # グラフ生成のメインロジック
├── Prefabs/                 # (想定) VertexPrefab, EdgePrefab など
├── Images/                  # (想定) UIやスプライト画像
└── Graph.md                 # 本ドキュメント
```

## 主なクラス

### 1. GraphGenerator (`Assets/Components/Game/Graph/Scripts/GraphGenerator.cs`)

グラフの生成と配置を担当する `MonoBehaviour` です。

#### 機能
*   **データ読み込み**: `StageDatabase` (ScriptableObject) から、指定されたステージインデックスのグラフデータを読み込みます。
*   **頂点生成**: `VertexData` に基づき、指定されたプレハブ (`VertexPrefab`) をインスタンス化して配置します。
    *   配置座標はグリッド座標 `(j, -i)` に基づき計算されます。
    *   頂点の名前にはタスク完了時間や担当ワーカーの情報が含まれます。
    *   頂点番号（1始まり）を子オブジェクトの `TextMeshPro` コンポーネントに設定します。
*   **辺生成**: `EdgeData` に基づき、指定されたプレハブ (`EdgePrefab`) をインスタンス化して配置します。
    *   2つの頂点の中点に配置され、頂点間を結ぶように回転・スケーリングされます。

#### インスペクター設定項目
*   **Global Settings**
    *   `Grid Spacing J`: 横方向（列）の間隔。
    *   `Grid Spacing I`: 縦方向（行）の間隔。
    *   `Edge Length Multiplier`: 辺の見た目の長さ倍率。
    *   `Vertex Prefab`: 頂点として生成するプレハブ。
    *   `Edge Prefab`: 辺として生成するプレハブ。
    *   `Graph Parent`: 生成されたオブジェクトの親となるTransform。
*   **Graph Data**
    *   `Stage Database`: ステージデータが格納された `StageDatabase` アセット。
    *   `Current Stage Index`: 読み込むステージのインデックス。

#### データ構造
*   **VertexData**: 頂点情報。
    *   `i`: 行インデックス（縦方向）。ワーカーの割り当て（A, B, C...）に対応します。
    *   `j`: 列インデックス（横方向）。時間軸的な順序を表します。
    *   `taskCompletionTime`: タスクの完了にかかる時間。
*   **EdgeData**: 辺情報。
    *   `fromIndex`: 始点の頂点インデックス。
    *   `toIndex`: 終点の頂点インデックス。

### 2. StageDatabase (`Assets/Components/Game/Database/StageDatabase.cs`)

ステージごとのデータを管理する `ScriptableObject` です。

*   `vertices`: `GraphGenerator.VertexData` のリスト。
*   `edges`: `GraphGenerator.EdgeData` のリスト。
*   `itemIds`: そのステージで使用されるアイテムIDのリスト（`ItemGenerator` で使用）。

## 使い方

1.  `Create > Game > StageDatabase` でデータベースアセットを作成し、各ステージのグラフデータを入力します。
2.  シーンに空のGameObjectを作成し、`GraphGenerator` コンポーネントをアタッチします。
3.  `GraphGenerator` のインスペクターで、`Vertex Prefab`, `Edge Prefab`, `Graph Parent` を設定します。
4.  `Stage Database` フィールドに作成したデータベースアセットを割り当て、`Current Stage Index` を指定します。
5.  ゲーム実行時（`Start` メソッド）に自動的にグラフが構築されます。

