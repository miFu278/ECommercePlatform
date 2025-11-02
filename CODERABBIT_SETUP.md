# CodeRabbit Setup - Quick Guide

## ⚠️ Important: CodeRabbit là GitHub App, KHÔNG phải GitHub Action

CodeRabbit hoạt động như một GitHub App và tự động review PRs. **KHÔNG cần workflow file.**

## Setup (2 bước)

### 1. Install CodeRabbit App

1. Vào: https://github.com/apps/coderabbitai
2. Click "Install"
3. Chọn repository
4. Authorize

### 2. Push Code

```bash
git add .
git commit -m "feat: setup CodeRabbit"
git push origin main
```

CodeRabbit sẽ tự động đọc `.coderabbit.yaml` và review PRs.

## Test

```bash
# Create test PR
git checkout -b test/coderabbit
echo "// Test" >> Program.cs
git commit -m "test: CodeRabbit"
git push origin test/coderabbit
# Create PR on GitHub
```

Đợi 1-2 phút, CodeRabbit sẽ comment.

## Commands

Trong PR comments:
- `@coderabbitai help` - Show commands
- `@coderabbitai review` - Request review
- `@coderabbitai explain` - Get explanation

## Configuration

File `.coderabbit.yaml` ở root chứa config.

## ❌ Không cần workflow

Nếu có file `.github/workflows/coderabbit.yml`, xóa đi:
```bash
rm .github/workflows/coderabbit.yml
```

CodeRabbit là App, không phải Action!

---

**Install:** https://github.com/apps/coderabbitai
