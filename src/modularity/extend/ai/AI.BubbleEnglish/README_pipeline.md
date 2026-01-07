# BubbleEnglish 视频分析链路（本地ASR + 本地TTS + Quartz）

## 你将得到什么
- **SourceText 自动获取**：ffmpeg 抽音频 -> whisper.cpp（faster-whisper 的本地替代方案）生成 `asr.txt + asr.srt`，写入 `bubble_video.SourceText`。
- **AI 多供应商**：支持 OpenAI / DeepSeek(兼容OpenAI) / 百度千帆（access_token）。
- **Units 音频**：
  - sentence：从 `audio.wav` 按 SRT 对齐切原声 mp3
  - word：本地 Piper TTS 输出 mp3
- **Quartz 后台任务**：ASR / AI / UnitAudio 全走后台 Job，可重跑、可审计。

## 本地文件路径规划（规范）
根目录：`Bubble:Storage:Root`

视频工作目录：`{Root}/bubble/videos/{yyyy}/{MM}/{videoId}/`
- `original.*`：原视频
- `derived/audio.wav`：16k mono wav
- `derived/subtitle/asr.txt`、`derived/subtitle/asr.srt`
- `derived/unit-audio/word_*.mp3`、`sentence_*.mp3`

对外URL：`Bubble:Storage:PublicPrefix`（例如 `/bubble/videos`），需要你在 Nginx/StaticFiles 映射到 `{Root}/bubble/videos`。

## 需要安装的本地工具
- ffmpeg（Windows/Linux 都可）
- whisper.cpp CLI（`whisper-cli`）+ 模型文件（例如 `ggml-base.en.bin`）
- piper（本地TTS） + 英文 voice 模型 onnx

## 配置示例（appsettings.json）
```json
{
  "Bubble": {
    "Storage": {
      "Root": "D:\\bubble-storage",
      "PublicPrefix": "/bubble/videos"
    },
    "Tools": {
      "FfmpegPath": "ffmpeg",
      "WhisperCliPath": "whisper-cli",
      "WhisperModelPath": "D:\\models\\ggml-base.en.bin",
      "WhisperThreads": 4,
      "WhisperLanguage": "en",
      "PiperPath": "piper",
      "PiperVoiceModelPath": "D:\\models\\en_US-amy-medium.onnx"
    },
    "AI": {
      "DefaultProvider": "openai",
      "OpenAi": { "BaseUrl": "https://api.openai.com", "ApiKey": "...", "DefaultModel": "gpt-4o-mini" },
      "DeepSeek": { "BaseUrl": "https://api.deepseek.com", "ApiKey": "...", "DefaultModel": "deepseek-chat" },
      "Qianfan": { "ApiKey": "...", "SecretKey": "...", "DefaultModelEndpoint": "ernie-bot-4" }
    }
  }
}
```

## 注册（Program.cs）
在你的主宿主项目（WebApi Host）里调用：
```csharp
builder.Services.AddBubbleEnglishPipeline(builder.Configuration);
```

## 后台接口（Admin）
- `POST /api/bubble/admin/pipeline/actions/asr` `{ videoId }`
- `POST /api/bubble/admin/pipeline/actions/analyze` `{ videoId, provider, model, prompt }`
- `POST /api/bubble/admin/pipeline/actions/unit-audio` `{ videoId }`
- `POST /api/bubble/admin/pipeline/actions/run` `{ videoId, provider, model, prompt }`
