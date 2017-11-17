using SharpNeat.View;
using SharpNeat.View.Graph;
using System;
using System.Collections;
using System.Drawing;
using ue = UnityEngine;
using System.IO;

namespace SimEditor
{
    public class GenomeDisplayer
    {
        private readonly Brush _brushBackground = new SolidBrush(Color.Lavender);

        private IOGraphViewportPainter _viewportPainter;

        private Image _image;
        private Rectangle _viewportArea;

        private int _width;
        private int _height;

        private float _zoomFactor = 1f;

        private NetworkGraphFactory _factory;
        private ue.Texture2D texture;

        public GenomeDisplayer()
        {
            _width = 500;
            _height = 500;

            _image = new Bitmap(_width, _height);
            _viewportPainter = new IOGraphViewportPainter(new IOGraphPainter());

            _viewportArea = new Rectangle(0, 0, _width, _height);
            _factory = new NetworkGraphFactory();
        }

        public ue.Sprite GetImage(SharpNeat.Network.INetworkDefinition net)
        {
            Graphics g = Graphics.FromImage(_image);
            g.FillRectangle(_brushBackground, 0, 0, _image.Width, _image.Height);

            _viewportPainter.IOGraph = _factory.CreateGraph(net);

            _viewportPainter.Paint(g, _viewportArea, _zoomFactor);

            texture = new ue.Texture2D(_width, _height, ue.TextureFormat.RGBA32, false);
            ImgToTexture(_image, texture);

            texture.Apply();

            var sprt = ue.Sprite.Create(texture, new ue.Rect(0, 0, _width, _height), ue.Vector2.zero);

            g.Dispose();

            return sprt;
        }

        private void ImgToTexture(Image imgToCopy, ue.Texture2D copyTo)
        {
            var bitmap = new Bitmap(imgToCopy);
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var pix = bitmap.GetPixel(x, y);
                    copyTo.SetPixel(x, y, ToUeColor(pix));
                }
            }
            imgToCopy = bitmap;
        }

        private ue.Color ToUeColor(Color c)
        {
            return new ue.Color(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        }
    }
}
