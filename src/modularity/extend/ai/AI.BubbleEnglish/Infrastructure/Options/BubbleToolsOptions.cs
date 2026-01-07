namespace AI.BubbleEnglish.Infrastructure.Options;

public class BubbleToolsOptions
{
    public string FfmpegPath { get; set; } = "ffmpeg";
    public string WhisperCliPath { get; set; } = "whisper-cli";
    public string WhisperModelPath { get; set; } = string.Empty;
    public int WhisperThreads { get; set; } = 4;
    public string WhisperLanguage { get; set; } = "en"; // en/auto

    public string PiperPath { get; set; } = "piper";
    public string PiperVoiceModelPath { get; set; } = string.Empty; // .onnx
}
